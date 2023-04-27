"use strict";
const express = require("express"),
    router = express.Router(),
    db = require('../db'),
    axios = require('axios'),
    moment = require('moment');
    require('dotenv').config();

// POST 
router.post("/rfid-scan", async (req, res, next) => {
    let rfid = req.body.Rfid;
    let autoReq = req.body.AutoReq;

    const rfidTagTypes = await db.pool.query(`
        SELECT dst_type, dst_display_title, dst_sheft_type, dst_shelf_name
        FROM drfid_special_tag 
        WHERE 
            dst_rfid_cd = '${rfid}'  `,
            ).catch(error => {
                console.log(error);
                return res.send({ success: false, message: error });
            }
    );

    let result;

    if(rfidTagTypes && rfidTagTypes.length > 0 && rfidTagTypes[0].dst_sheft_type == "SHELF") {
        console.log("specific tag");
        result = await specialTagHandle(req.io, rfidTagTypes[0].dst_type, rfidTagTypes[0].dst_display_title, rfidTagTypes[0].dst_shelf_name, rfid, autoReq);
    } else if(rfidTagTypes && rfidTagTypes.length > 0 && rfidTagTypes[0].dst_sheft_type == "TABLE") {
        console.log("table tag");
        result = await tableTagHandle(req.io, rfidTagTypes[0].dst_type, rfidTagTypes[0].dst_display_title, rfidTagTypes[0].dst_shelf_name, rfid);
    } else{
        const janByRfids = await db.poolPcStore.query(`
            SELECT drgm_jan
            FROM drfid_rfgoods_master 
            WHERE 
                drgm_rfid_cd = '${rfid}'  `,
                ).catch(error => {
                    console.log(error);
                    return res.send({ success: false, message: error });
                }
        );

        const janTypes = await db.pool.query(`
            SELECT dcv_video_url
            FROM drfid_cm_video 
            WHERE 
                dcv_jan_cd = '${janByRfids[0]?.drgm_jan}'  `,
            ).catch(error => {
                console.log(error);
                return res.send({ success: false, message: error });
            }
        );
        
        if(janTypes && janTypes.length > 0) {
            console.log("cm tag");
            result = await cmTagHandle(rfid, req.io, janTypes);
        } else {
            console.log("common tag");
            result = await commonTagHandle(rfid, req.io, autoReq);
        }
    }

    //send result to client
    if(result.success) {
        res.send({ success: true, message: result.message });
    } else {
        res.send({ success: false, message: result.message });
    }
});

async function specialTagHandle(io, rfidTagType, displayTitle, shelfName, rfid, autoReq) {
    try {
        var whereStr, funcCountStr;
        console.log(rfidTagType);
        switch(rfidTagType) {
            case "DAY":
                funcCountStr = "count";
                whereStr = `dlm_date = "${moment().format("YYYY-MM-DD")}"`;
                break;
            case "WEEK":
                funcCountStr = "count";
                var startOfWeek = moment().subtract(6, 'days').format('YYYY-MM-DD');
                whereStr = `dlm_date BETWEEN "${startOfWeek}" AND "${moment().format("YYYY-MM-DD")}"`;
                break;
            case "MONTH":
                funcCountStr = "count";
                var startOfMonth = moment().subtract(29, 'days').format('YYYY-MM-DD');
                whereStr = `dlm_date BETWEEN "${startOfMonth}" AND "${moment().format("YYYY-MM-DD")}"`;
                break;
            case "ALL":
                funcCountStr = "count";
                whereStr = "1 = 1";
                break;
            default:
                return { success: false, message: "RFID tag error" };
        }

        const result = await db.pool.query(`
                SELECT 
                    CONVERT(ROW_NUMBER() OVER (ORDER BY temp.view_time_in_seconds DESC, temp.view_cnt DESC), CHAR) AS "rank",
                    temp.*
                FROM 
                    (SELECT
                        CONVERT(ifnull(${funcCountStr}(dlm_cnt), 0), CHAR) AS view_cnt,
                        ifnull(sum(time_to_sec(timediff(dlm_indate, dlm_outdate))), 0) AS view_time_in_seconds,
                        dpp_isbn as isbn,
                        dpp_shelf_pos as shelf_pos,
                        dpp_shelf_col_pos as shelf_col_pos,
                        dpp_product_name as name,
                        dpp_image_url as img
                    FROM 
                        drfid_product_pos pos LEFT JOIN 
                        (
                            SELECT * 
                            FROM drfid_log_move 
                            WHERE 
                                dlm_cnt > 0
                                AND dlm_indate is not null
                                AND dlm_outdate is not null
                                AND ifnull(time_to_sec(timediff(dlm_indate, dlm_outdate)), 0) >= 10
                                AND ${whereStr}
                        ) mv ON mv.dlm_rfid_cd = pos.dpp_rfid_cd
                    WHERE 
                        ifnull(pos.dpp_rfid_cd, '') != '' AND
                        dpp_shelf_name = "${shelfName}"
                    GROUP BY 
                        dpp_rfid_cd) as temp
                ORDER BY
                        temp.shelf_pos, temp.shelf_col_pos  `,
            ).catch(error => {
                console.log(error);
                return { success: false, message: error };
            }
        );

        if(result.length > 0) {
            let response = [];
            for (const iterator of result) {
                if(iterator.img == null || iterator.img == "") {
                    iterator.img = (await getImage([iterator.isbn]))[iterator.isbn] ?? "";
                }
                // } else {
                //     iterator.img = iterator.img;
                // }

                response.push({
                    "rank" : iterator.rank, 
                    "view_cnt" : iterator.view_cnt, 
                    "view_time_in_seconds" : iterator.view_time_in_seconds, 
                    "shelf_pos" : iterator.shelf_pos,
                    "shelf_col_pos" : iterator.shelf_col_pos,
                    "name" : iterator.name,
                    "img" : iterator.img
                });
            }

            for (let row = 0; row < process.env.SHELF_ROW_SIZE; row++) {
                for (let col = 0; col < process.env.SHELF_COL_SIZE; col++) {
                    //current shelf position not available, then insert null row
                    let idxResponse = process.env.SHELF_COL_SIZE * row + col;
                    if (typeof response[idxResponse] === 'undefined'
                        || response[idxResponse].shelf_pos != row + 1
                        || response[idxResponse].shelf_col_pos != col + 1) {
                        //insert null row
                        response.splice(idxResponse, 0, null);
                    }
                }
            }
            console.log(response);
            io.emit("displayScreen", { type: "SMART_SHELF", title: displayTitle, data : response, rfid: rfid, autoReq: autoReq });
        } else {
            return { success: false, message: "no data" };
        }

    } catch (err) {
        console.log(err);
        return { success: false, message: err._message };
    }

    return { success: true, message: "success" };
}

async function commonTagHandle(rfidTag, io, autoReq) {
    try {
        //handle get media cd 3
        const result = await db.poolPcStore.query(`
                SELECT 
                    drgm_c_code as mc3
                FROM 
                    drfid_rfgoods_master
                WHERE
                    drgm_rfid_cd = '${rfidTag}' `,
            ).catch(error => {
                console.log(error);
                return { success: false, message: error };
            }
        );

        if(result.length == 0 || result[0].mc3 == null || result[0].mc3 == "") {
            return { success: false, message: "no data" };
        }

        await axios.get(process.env.GCE_HOST + '/api/best-seller?mc3=' + result[0].mc3)
            .then(async response => {
                if(response.data.success) {
                    if(response.data.data == null) {
                        io.emit("displayScreen", { type: "DIGITAL_SIGNAGE", data : [], rfid: rfidTag, autoReq: autoReq });
                        return { success: false, message: "no data" };
                    }
                    
                    const resultInfo = await db.pool.query(`
                            SELECT CONCAT('<br>', description) as 'description' FROM smart_shelf.ccode_info where SUBSTRING('${result[0].mc3}', 1, 1) = CAST(digit as INT) AND SUBSTRING('${result[0].mc3}', 1, 1) != '' AND type = 1
                            UNION
                            SELECT CONCAT('<br>', description) as 'description' FROM smart_shelf.ccode_info where SUBSTRING('${result[0].mc3}', 2, 1) = CAST(digit as INT) AND SUBSTRING('${result[0].mc3}', 2, 1) != '' AND type = 2
                            UNION
                            SELECT CONCAT('<br>', description) as 'description' FROM smart_shelf.ccode_info where SUBSTRING('${result[0].mc3}', 3, 2) = CAST(digit as INT) AND SUBSTRING('${result[0].mc3}', 3, 2) != '' AND type = 3  `,
                        ).catch(error => {
                            console.log(error);
                            return { success: false, message: error };
                        }
                    );

                    if(resultInfo.length > 0) {
                        var cCodeInfo = resultInfo.map(item => item.description).join("");
                    }
                    
                    io.emit("displayScreen", { type: "DIGITAL_SIGNAGE", data : response.data.data, rfid: rfidTag, autoReq: autoReq, cCodeInfo: "(検索条件)" + (cCodeInfo ?? "") });
                    console.log( { type: "DIGITAL_SIGNAGE", data : response.data, rfid: rfidTag});
                } else {
                    return { success: false, message: response.data.message };
                }
            })
            .catch(error => {
                console.log(error);
                return { success: false, message: error };
            });
    } catch (error) {
        console.log(error);
        return { success: false, message: error._message };
    }

    return { success: true, message: "success" };
}

async function tableTagHandle(io, rfidTagType, displayTitle, shelfName, rfid) {
    try {
        var whereStr, funcCountStr;
        console.log(rfidTagType);
        switch(rfidTagType) {
            case "DAY":
                funcCountStr = "count";
                whereStr = `dlm_date = "${moment().format("YYYY-MM-DD")}"`;
                break;
            case "WEEK":
                funcCountStr = "count";
                var startOfWeek = moment().subtract(6, 'days').format('YYYY-MM-DD');
                whereStr = `dlm_date BETWEEN "${startOfWeek}" AND "${moment().format("YYYY-MM-DD")}"`;
                break;
            case "MONTH":
                funcCountStr = "count";
                var startOfMonth = moment().subtract(29, 'days').format('YYYY-MM-DD');
                whereStr = `dlm_date BETWEEN "${startOfMonth}" AND "${moment().format("YYYY-MM-DD")}"`;
                break;
            case "ALL":
                funcCountStr = "count";
                whereStr = "1 = 1";
                break;
            default:
                return { success: false, message: "RFID tag error" };
        }

        const result = await db.pool.query(`
                SELECT 
                    CONVERT(ROW_NUMBER() OVER (ORDER BY temp.view_time_in_seconds DESC, temp.view_cnt DESC), CHAR) AS "rank",
                    temp.*
                FROM 
                    (SELECT
                        CONVERT(ifnull(${funcCountStr}(dlm_cnt), 0), CHAR) AS view_cnt,
                        ifnull(sum(time_to_sec(timediff(dlm_indate, dlm_outdate))), 0) AS view_time_in_seconds,
                        dpp_isbn as isbn,
                        dpp_shelf_pos as shelf_pos,
                        dpp_shelf_col_pos as shelf_col_pos,
                        dpp_product_name as name
                    FROM 
                        drfid_product_pos pos LEFT JOIN 
                        (
                            SELECT * 
                            FROM drfid_log_move 
                            WHERE 
                                dlm_cnt > 0
                                AND dlm_indate is not null
                                AND dlm_outdate is not null
                                AND ifnull(time_to_sec(timediff(dlm_indate, dlm_outdate)), 0) >= 10
                                AND ${whereStr}
                        ) mv ON mv.dlm_rfid_cd = pos.dpp_rfid_cd
                    WHERE 
                        dpp_shelf_name = "${shelfName}"
                    GROUP BY 
                        dpp_jan_cd, dpp_isbn, dpp_shelf_pos, dpp_shelf_col_pos, dpp_product_name) as temp
                ORDER BY
                        temp.shelf_pos, temp.shelf_col_pos  `,
            ).catch(error => {
                console.log(error);
                return { success: false, message: error };
            }
        );

        if(result.length > 0) {
            let response = [];

            const isbnHash = await getImage(result.map(item => item.isbn));
            console.log(result.map(item => item.isbn));
            console.log(isbnHash);
            for (const iterator of result) {
                response.push({
                    "rank" : iterator.rank, 
                    "view_cnt" : iterator.view_cnt, 
                    "view_time_in_seconds" : iterator.view_time_in_seconds, 
                    "shelf_pos" : iterator.shelf_pos,
                    "shelf_col_pos" : iterator.shelf_col_pos,
                    "name" : iterator.name,
                    "img" : isbnHash[iterator.isbn] ?? ""
                });
            }
            console.log(response);
            for (let row = 0; row < process.env.TABLE_ROW_SIZE; row++) {
                for (let col = 0; col < process.env.TABLE_COL_SIZE; col++) {
                    //current shelf position not available, then insert null row
                    let idxResponse = process.env.TABLE_COL_SIZE * row + col;
                    if (typeof response[idxResponse] === 'undefined'
                        || response[idxResponse].shelf_pos != row + 1
                        || response[idxResponse].shelf_col_pos != col + 1) {
                        //insert null row
                        response.splice(idxResponse, 0, null);
                    }
                }
            }
            
            //console.log(response);
            io.emit("displayScreen", { type: "SMART_TABLE", title: displayTitle, data : response, rfid: rfid });
        } else {
            return { success: false, message: "no data" };
        }

    } catch (err) {
        console.log(err);
        return { success: false, message: err._message };
    }

    return { success: true, message: "success" };
}

async function cmTagHandle(rfidTag, io, videoUrl) {
    try {
        //handle get media cd 3
        const result = await db.poolPcStore.query(`
                SELECT 
                    drgm_c_code as mc3
                FROM 
                    drfid_rfgoods_master
                WHERE
                    drgm_rfid_cd = '${rfidTag}' `,
            ).catch(error => {
                console.log(error);
                return { success: false, message: error._message };
            }
        );

        if(result.length == 0 || result[0].mc3 == null || result[0].mc3 == "") {
            return { success: false, message: "no data" };
        }

        await axios.get(process.env.GCE_HOST + '/api/best-seller?mc3=' + result[0].mc3)
            .then(async response => {
                if(response.data.success) {
                    const resultInfo = await db.pool.query(`
                            SELECT CONCAT('<br>', description) as 'description' FROM smart_shelf.ccode_info where SUBSTRING('${result[0].mc3}', 1, 1) = CAST(digit as INT) AND SUBSTRING('${result[0].mc3}', 1, 1) != '' AND type = 1
                            UNION
                            SELECT CONCAT('<br>', description) as 'description' FROM smart_shelf.ccode_info where SUBSTRING('${result[0].mc3}', 2, 1) = CAST(digit as INT) AND SUBSTRING('${result[0].mc3}', 2, 1) != '' AND type = 2
                            UNION
                            SELECT CONCAT('<br>', description) as 'description' FROM smart_shelf.ccode_info where SUBSTRING('${result[0].mc3}', 3, 2) = CAST(digit as INT) AND SUBSTRING('${result[0].mc3}', 3, 2) != '' AND type = 3  `,
                        ).catch(error => {
                            console.log(error);
                            return { success: false, message: error };
                        }
                    );

                    if(resultInfo.length > 0) {
                        var cCodeInfo = resultInfo.map(item => item.description).join("");
                    }
                    
                    io.emit("displayScreen", { type: "CM", data : response.data.data, videoUrl: videoUrl, rfid: rfidTag, cCodeInfo: "(検索条件)" + (cCodeInfo ?? "")});
                    console.log( { type: "CM", data : response.data.data, videoUrl: videoUrl, rfid: rfidTag});
                } else {
                    return { success: false, message: response.data.message };
                }
            })
            .catch(error => {
                console.log(error);
                return { success: false, message: error._message };
            });
        
    } catch (err) {
        console.log(err);
        return { success: false, message: err._message };
    }
    return { success: true, message: "success" };
}

async function getImage(isbns) {
    try {
        let hashObj = {};
        const result = await axios.get('https://api.openbd.jp/v1/get?isbn=' + isbns.join(','));
        for (let index = 0; index < isbns.length; index++) {
            hashObj[isbns[index]] = result.data[index]?.summary.cover;
        }
        // for (let index = 0; index < isbns.length; index++) {
        //     const result = await axios.post(`${process.env.IMAGE_API_HOST}/api/v1/get_image_BQ`, {api_key: process.env.IMAGE_API_KEY, isbn: isbns[index]});
        //     hashObj[isbns[index]] = result.data?.base64 != null && result.data?.base64 != ""
        //                             ? "data:image/jpeg;base64," +  result.data?.base64 
        //                             : result.data?.cover;
        // }
        return hashObj;
    } catch (err) {
        console.log(err);
        return null;
    }
};

module.exports = router;