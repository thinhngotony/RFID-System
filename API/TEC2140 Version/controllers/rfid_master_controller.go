package controllers

import (
	"main/db_client"
	"main/utils"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/go-playground/validator"
)

type InsertRFIDMaster struct {
	ApiKey       string `json:"api_key" validate:"required"`
	Force_update *bool  `json:"force_update" validate:"required"`

	Drgm_rfid_cd  string `json:"drgm_rfid_cd" validate:"required"`
	Drgm_artist   string `json:"drgm_artist" `
	Drgm_maker_cd string `json:"drgm_maker_cd" `

	Rf_goods_type string `json:"rf_goods_type" `

	Drgm_price_tax_off int    `json:"drgm_price_tax_off"`
	Drgm_jan           string `json:"drgm_jan" validate:"required"`
	Drgm_jan2          string `json:"drgm_jan2"`
	Drgm_goods_name    string `json:"drgm_goods_name" `
	Drgm_media_cd      string `json:"drgm_media_cd"`

	Rf_goods_cd_type string `json:"rf_goods_cd_type"`
	Drgm_pos_shop_cd string `json:"drgm_pos_shop_cd"`

	Drgm_goods_name_kana string  `json:"drgm_goods_name_kana" `
	Drgm_artist_kana     string  `json:"drgm_artist_kana"`
	Drgm_maker_name      string  `json:"drgm_maker_name"`
	Drgm_genre_cd        string  `json:"drgm_genre_cd"`
	Drgm_maker_name_kana string  `json:"drgm_maker_name_kana" `
	Drgm_c_code          string  `json:"drgm_c_code" `
	Drgm_selling_date    string  `json:"drgm_selling_date" `
	Drgm_cost_rate       float64 `json:"drgm_cost_rate" `
	Drgm_cost_price      int     `json:"drgm_cost_price" `
}

func InsertDataToMasterTable(c *gin.Context) {
	var reqBody InsertRFIDMaster
	var PosShopTemp string
	var RfGoodsCodeTypeTemp string

	if reqBody.Drgm_pos_shop_cd != "" {
		PosShopTemp = reqBody.Drgm_pos_shop_cd
	} else {
		PosShopTemp = "POSTEST"
	}

	if reqBody.Rf_goods_cd_type != "" {
		RfGoodsCodeTypeTemp = reqBody.Rf_goods_cd_type
	} else {
		RfGoodsCodeTypeTemp = "1"
	}

	if err := c.ShouldBindJSON(&reqBody); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.INVALID_FORMAT_CODE,
			"data":    nil,
			"message": "jsonリクエストの本文の形式が正しくありません。",
		})
		return

	} else if err := validator.New().Struct(reqBody); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.MISSING_DATA_CODE,
			"data":    nil,
			"message": "リクエストパラメータが不足しています。",
		})
		return

	} else if err := utils.VerifyApiKey(reqBody.ApiKey); !err {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.INVALID_API_KEY_CODE,
			"data":    nil,
			"message": "APIキーが正しくありません。",
		})
		return
	}

	db, _ := db_client.DbConnection()
	defer db.Close()

	if data_exist, _, _ := db_client.CheckExistRFID_Master(db, []string{reqBody.Drgm_rfid_cd}); len(data_exist) == 0 {
		if create_date, shop_code, err := db_client.InsertMaster(db,
			PosShopTemp,
			RfGoodsCodeTypeTemp,
			reqBody.Rf_goods_cd_type,
			reqBody.Drgm_rfid_cd,
			reqBody.Drgm_jan,
			reqBody.Drgm_jan2,
			reqBody.Drgm_goods_name,
			reqBody.Drgm_goods_name_kana,
			reqBody.Drgm_artist,
			reqBody.Drgm_artist_kana,
			reqBody.Drgm_maker_cd,
			reqBody.Drgm_maker_name,
			reqBody.Drgm_genre_cd,
			reqBody.Drgm_maker_name_kana,
			reqBody.Drgm_c_code,
			reqBody.Drgm_selling_date,
			reqBody.Drgm_price_tax_off,
			reqBody.Drgm_cost_rate,
			reqBody.Drgm_cost_price,
			reqBody.Drgm_media_cd); err != nil {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.UNKNOWN_ERROR_CODE,
				"data":    nil,
				"message": "システムの不明なエラーです。",
			})

		} else {
			data := gin.H{
				"create_date": create_date,
				"shop_code":   shop_code,
				"drgm_jan":    reqBody.Drgm_jan,
			}
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.SUCCESSFULLY_CODE,
				"data":    data,
				"message": "登録に成功しました。",
			})
		}
	} else if len(data_exist) != 0 && !*reqBody.Force_update {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.DATA_EXISTED_CODE,
			"data":    nil,
			"message": "データベースでRFIDコードが重複しています。",
		})

	} else if *reqBody.Force_update {

		if create_date, shop_code, err := db_client.UpdateMaster(db,
			PosShopTemp,
			RfGoodsCodeTypeTemp,
			reqBody.Rf_goods_cd_type,
			reqBody.Drgm_rfid_cd,
			reqBody.Drgm_jan,
			reqBody.Drgm_jan2,
			reqBody.Drgm_goods_name,
			reqBody.Drgm_goods_name_kana,
			reqBody.Drgm_artist,
			reqBody.Drgm_artist_kana,
			reqBody.Drgm_maker_cd,
			reqBody.Drgm_maker_name,
			reqBody.Drgm_genre_cd,
			reqBody.Drgm_maker_name_kana,
			reqBody.Drgm_c_code,
			reqBody.Drgm_selling_date,
			reqBody.Drgm_price_tax_off,
			reqBody.Drgm_cost_rate,
			reqBody.Drgm_cost_price,
			reqBody.Drgm_media_cd); err != nil {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.UNKNOWN_ERROR_CODE,
				"data":    nil,
				"message": "システムの不明なエラーです。",
			})
		} else {
			data := gin.H{
				"create_date": create_date,
				"shop_code":   shop_code,
				"drgm_jan":    reqBody.Drgm_jan,
			}
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.SUCCESSFULLY_CODE,
				"data":    data,
				"message": "登録に成功しました。",
			})
		}

	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"data":    nil,
			"message": "システムの不明なエラーです。",
		})
	}
}

func DeleteDataFromMasterTable(c *gin.Context) {
	var reqBody RFIDList
	if err := c.ShouldBindJSON(&reqBody); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.INVALID_FORMAT_CODE,
			"data":    nil,
			"message": "jsonリクエストの本文の形式が正しくありません。",
		})
		return

	} else if err := validator.New().Struct(reqBody); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.MISSING_DATA_CODE,
			"data":    nil,
			"message": "リクエストパラメータが不足しています。",
		})
		return

	} else if err := utils.VerifyApiKey(reqBody.ApiKey); !err {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.INVALID_API_KEY_CODE,
			"data":    nil,
			"message": "APIキーが正しくありません。",
		})
		return
	}

	db, _ := db_client.DbConnection()

	defer db.Close()

	rfid_list, empty_rfid, _ := db_client.CheckExistRFID_Master(db, reqBody.RFIDList)

	if len(empty_rfid) != 0 {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.DATA_NOT_EXIST_CODE,
			"data":    rfid_list,
			"error":   empty_rfid,
			"message": "不正なRFIDコードが存在しています。",
		})

	} else if len(empty_rfid) == 0 && len(rfid_list) != 0 {
		if _, err := db_client.DeleteMaster(db, reqBody.RFIDList); err != nil {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.UNKNOWN_ERROR_CODE,
				"data":    nil,
				"error":   nil,
				"message": "システムの不明なエラーです。",
			})
		} else {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.SUCCESSFULLY_CODE,
				"data":    rfid_list,
				"error":   nil,
				"message": "データの削除に成功しました。",
			})

		}
	}
}
