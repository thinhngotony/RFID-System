package db_client

import (
	"context"
	"database/sql"
	"fmt"
	"log"
	"main/utils"
	"strings"
	"time"

	_ "github.com/go-sql-driver/mysql"
)

type RFID_Jan struct {
	RFID      string `json:"rfid"`
	JanCode_1 string `json:"jancode_1"`
	JanCode_2 string `json:"jancode_2"`
}

type RFID_Exist struct {
	RFID_Exist     string `json:"rfid_exist"`
	RFID_Not_Exist string `json:"rfid_not_exist"`
}

type RFID_Status struct {
	RFID   string `json:"rfid"`
	Status string `json:"status"`
}

type Gate_Setting struct {
	UGC           *int    `json:"use_gate_checkpoint"`
	IP            *string `json:"url_gate_checkpoint"`
	Username_gate *string `json:"username_gate"`
	Password_gate *string `json:"password_gate"`
}

type Data_Search struct {
	Drgm_create          *string  `json:"drgm_create"`
	Drgm_pos_shop_cd     *string  `json:"drgm_pos_shop_cd"`
	Drgm_com_shop_cd     *string  `json:"drgm_com_shop_cd"`
	Rf_goods_type        *string  `json:"rf_goods_type"`
	Rf_goods_cd_type     *string  `json:"rf_goods_cd_type"`
	Drgm_rfid_cd         *string  `json:"drgm_rfid_cd"`
	Drgm_jan             *string  `json:"drgm_jan"`
	Drgm_jan2            *string  `json:"drgm_jan2"`
	Drgm_goods_name      *string  `json:"drgm_goods_name"`
	Drgm_goods_name_kana *string  `json:"drgm_goods_name_kana"`
	Drgm_artist          *string  `json:"drgm_artist"`
	Drgm_artist_kana     *string  `json:"drgm_artist_kana"`
	Drgm_maker_cd        *string  `json:"drgm_maker_cd"`
	Drgm_maker_name      *string  `json:"drgm_maker_name"`
	Drgm_genre_cd        *string  `json:"drgm_genre_cd"`
	Drgm_maker_name_kana *string  `json:"drgm_maker_name_kana"`
	Drgm_c_code          *string  `json:"drgm_c_code"`
	Drgm_selling_date    *string  `json:"drgm_selling_date"`
	Drgm_price_tax_off   *int     `json:"drgm_price_tax_off"`
	Drgm_cost_rate       *float64 `json:"drgm_cost_rate"`
	Drgm_cost_price      *int     `json:"drgm_cost_price"`
	Drgm_media_cd        *string  `json:"drgm_media_cd"`

	Bqsg_shop_goods_price       *float64 `json:"bqsq_shop_goods_price"`
	Bqsg_shop_goods_price_intax *float64 `json:"bqsq_shop_goods_price_intax"`
}

type InsertTransaction struct {
	ApiKey           string `json:"api_key" validate:"required"`
	Drtr_receipt_no  string `json:"drtr_receipt_no"`
	Drtr_shop_no     string `json:"drtr_shop_no"`
	Drtr_pos_no      string `json:"drtr_pos_no"`
	Drtr_date_create string `json:"drtr_date_create"`
	Drtr_start_time  string `json:"drtr_start_time"`
	Drtr_end_time    string `json:"drtr_end_time"`
	Drtr_is_saved    string `json:"drtr_is_saved"`
}

func DbConnection() (*sql.DB, error) {
	Username, Password, Hostname, Dbname := utils.LoadDatabase()
	db, err := sql.Open("mysql", fmt.Sprintf("%s:%s@tcp(%s)/%s", Username, Password, Hostname, Dbname))
	if err != nil {
		log.Printf("Error %s when opening DB\n", err)
		return nil, err
	}
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	err = db.PingContext(ctx)
	if err != nil {
		log.Printf("Errors %s pinging database", err)
		return nil, err
	}
	return db, nil
}

func ConvertFromSingleRFID(db *sql.DB, rfid string) (string, string, bool, error) {
	log.Printf("Getting JAN code")
	query := `select drgm_jan, drgm_jan2 from drfid_rfgoods_master where drgm_rfid_cd = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return "", "", false, err
	}
	defer stmt.Close()
	var drgm_jan, jancode_2 string
	row := stmt.QueryRowContext(ctx, rfid)
	if err := row.Scan(&drgm_jan, &jancode_2); err != nil {

		return "", "", false, err
	}
	return drgm_jan, jancode_2, true, nil

}

func GetInfoFromSingleRFID(db *sql.DB, rfid string) (Data_Search, bool, error) {
	log.Printf("Getting information from RFID")
	query := `select drgm_create,
	drgm_pos_shop_cd,
	drgm_com_shop_cd,
	rf_goods_type,
	rf_goods_cd_type,
	drgm_rfid_cd,
	drgm_jan,
	drgm_jan2,
	drgm_goods_name,
	drgm_goods_name_kana,
	drgm_artist,
	drgm_artist_kana,
	drgm_maker_cd,
	drgm_maker_name,
	drgm_genre_cd,
	drgm_maker_name_kana,
	drgm_c_code,
	drgm_selling_date,
	drgm_price_tax_off,
	drgm_cost_rate,
	drgm_media_cd
	from drfid_rfgoods_master where drgm_rfid_cd = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return Data_Search{}, false, err
	}
	defer stmt.Close()
	var data Data_Search
	row := stmt.QueryRowContext(ctx, rfid)
	if err := row.Scan(&data.Drgm_create,
		&data.Drgm_pos_shop_cd,
		&data.Drgm_com_shop_cd,
		&data.Rf_goods_type,
		&data.Rf_goods_cd_type,
		&data.Drgm_rfid_cd,
		&data.Drgm_jan,
		&data.Drgm_jan2,
		&data.Drgm_goods_name,
		&data.Drgm_goods_name_kana,
		&data.Drgm_artist,
		&data.Drgm_artist_kana,
		&data.Drgm_maker_cd,
		&data.Drgm_maker_name,
		&data.Drgm_genre_cd,
		&data.Drgm_maker_name_kana,
		&data.Drgm_c_code,
		&data.Drgm_selling_date,
		&data.Drgm_price_tax_off,
		&data.Drgm_cost_rate,
		&data.Drgm_media_cd); err != nil {
		return Data_Search{}, false, err
	}
	return data, true, nil

}

func ConvertFromRFID(db *sql.DB, rfid string) ([]RFID_Jan, error) {
	log.Printf("Getting JAN code from list of RFID")
	query := `select drgm_rfid_cd, drgm_jan, drgm_jan2 from drfid_rfgoods_master where drgm_rfid_cd = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return []RFID_Jan{}, err
	}
	defer stmt.Close()
	rows, err := stmt.QueryContext(ctx, rfid)
	if err != nil {
		return []RFID_Jan{}, err
	}
	defer rows.Close()

	var rfid_list = []RFID_Jan{}
	for rows.Next() {
		var prd RFID_Jan
		if err := rows.Scan(&prd.RFID, &prd.JanCode_1, &prd.JanCode_2); err != nil {
			return []RFID_Jan{}, err
		}
		rfid_list = append(rfid_list, prd)
	}
	if err := rows.Err(); err != nil {
		return []RFID_Jan{}, err
	}
	return rfid_list, nil
}

func CheckRFIDStatus(db *sql.DB, rfid string) ([]RFID_Status, []RFID_Status, error) {
	log.Printf("Getting JAN code from list of RFID")
	query := `select dt_rfid_cd, dt_mode from drfid_taglog where dt_rfid_cd = ? order by dt_create_date desc LIMIT 1;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return []RFID_Status{}, []RFID_Status{}, err
	}
	defer stmt.Close()
	rows, err := stmt.QueryContext(ctx, rfid)
	if err != nil {
		return []RFID_Status{}, []RFID_Status{}, err
	}
	defer rows.Close()

	var rfid_status_list = []RFID_Status{}
	var rfid_status_invalid_list = []RFID_Status{}

	for rows.Next() {

		var prd RFID_Status
		if err := rows.Scan(&prd.RFID, &prd.Status); err != nil {
			return []RFID_Status{}, []RFID_Status{}, err
		}

		if prd.Status == "00" {
			rfid_status_list = append(rfid_status_list, prd)

		} else {
			rfid_status_invalid_list = append(rfid_status_invalid_list, prd)
		}
	}
	if err := rows.Err(); err != nil {
		return []RFID_Status{}, []RFID_Status{}, err
	}
	return rfid_status_list, rfid_status_invalid_list, nil
}

func CheckExistRFID_Master(db *sql.DB, rfid_list []string) ([]string, []string, error) {
	log.Printf("Checking RFID in database from list of RFID")
	query := `select drgm_rfid_cd from drfid_rfgoods_master where drgm_rfid_cd = ? ;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return nil, nil, err
	}
	defer stmt.Close()

	var rfid_exist []string
	var rfid_not_exist []string

	for _, rfid := range rfid_list {
		rows, err := stmt.QueryContext(ctx, rfid)
		if err != nil {
			return nil, nil, err
		}
		defer rows.Close()
		for rows.Next() {
			var prd RFID_Exist
			if err := rows.Scan(&prd.RFID_Exist); err != nil {
				return nil, nil, err
			}
			rfid_exist = append(rfid_exist, prd.RFID_Exist)
		}

		if err := rows.Err(); err != nil {
			return nil, nil, err
		}
	}

	rfid_not_exist = DifferenceSlice(rfid_list, rfid_exist)

	return rfid_exist, rfid_not_exist, nil
}

func RemoveDuplicateStr(strSlice []string) []string {
	allKeys := make(map[string]bool)
	list := []string{}
	for _, item := range strSlice {
		if _, value := allKeys[item]; !value {
			allKeys[item] = true
			list = append(list, item)
		}
	}
	return list
}

func GetDataFromRFIDList(db *sql.DB, RFIDList []string) ([]RFID_Jan, []string) {
	var data []RFID_Jan
	var empty_rfid []string
	RFIDListValid := RemoveDuplicateStr(RFIDList)
	for i := 0; i < len(RFIDListValid); i++ {
		if rfid, _ := ConvertFromRFID(db, RFIDListValid[i]); len(rfid) == 0 {
			empty_rfid = append(empty_rfid, RFIDListValid[i])
		} else {
			data = append(data, rfid...)
		}
	}
	return data, empty_rfid
}

func DifferenceSlice(slice1 []string, slice2 []string) []string {
	diffStr := []string{}
	m := map[string]int{}

	for _, s1Val := range slice1 {
		m[s1Val] = 1
	}
	for _, s2Val := range slice2 {
		m[s2Val] = m[s2Val] + 1
	}

	for mKey, mVal := range m {
		if mVal == 1 {
			diffStr = append(diffStr, mKey)
		}
	}

	return diffStr
}

func GetStatusFromRFIDList(db *sql.DB, RFIDList []string) ([]RFID_Status, []RFID_Status, []string) {
	var rfid_sold_list []RFID_Status
	var rfid_unsold_list []RFID_Status
	var unknown_rfid_list []string
	var list []string

	RFIDListValid := RemoveDuplicateStr(RFIDList)

	for i := 0; i < len(RFIDListValid); i++ {

		if rfid_sold, rfid_unsold, _ := CheckRFIDStatus(db, RFIDListValid[i]); len(rfid_sold) == 0 && len(rfid_unsold) == 0 {
			unknown_rfid_list = append(unknown_rfid_list, RFIDListValid[i])

		} else {
			rfid_sold_list = append(rfid_sold_list, rfid_sold...)
			rfid_unsold_list = append(rfid_unsold_list, rfid_unsold...)
		}
	}

	//Check unknown_rfid_list exists in master table, if exist move to rfid_unsold_list
	if len(unknown_rfid_list) != 0 {
		for i := 0; i < len(unknown_rfid_list); i++ {
			if _, _, exist, _ := ConvertFromSingleRFID(db, unknown_rfid_list[i]); exist {
				list = append(list, unknown_rfid_list[i])
				rfid_unsold_list = append(rfid_unsold_list, RFID_Status{
					RFID:   unknown_rfid_list[i],
					Status: "",
				})
			}
		}
	}

	//Remove in unknown_rfid_list
	unknown_rfid_list = DifferenceSlice(list, unknown_rfid_list)

	return rfid_sold_list, rfid_unsold_list, unknown_rfid_list
}

func ConvertFromJan1(db *sql.DB, Drgm_jan string) ([]string, bool, error) {
	log.Printf("Getting list of RFID from Jan code 1")
	query := `select drgm_rfid_cd from drfid_rfgoods_master where drgm_jan = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return nil, false, err
	}
	defer stmt.Close()

	rows, err := stmt.QueryContext(ctx, Drgm_jan)
	if err != nil {
		return nil, false, err
	}
	defer rows.Close()

	var rfid_list []string
	for rows.Next() {
		var prd string
		if err := rows.Scan(&prd); err != nil {
			return nil, false, err
		}
		rfid_list = append(rfid_list, prd)
	}

	if err := rows.Err(); err != nil {
		return nil, false, err
	}

	if len(rfid_list) == 0 {
		return nil, false, err
	}

	return rfid_list, true, nil
}

func ConvertFromJan2(db *sql.DB, jancode_2 string) ([]string, bool, error) {
	log.Printf("Getting list of RFID from Jan code 2")
	query := `select drgm_rfid_cd from drfid_rfgoods_master where drgm_jan2 = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return nil, false, err
	}
	defer stmt.Close()

	rows, err := stmt.QueryContext(ctx, jancode_2)
	if err != nil {
		return nil, false, err
	}
	defer rows.Close()

	var rfid_list []string
	for rows.Next() {
		var items string
		if err := rows.Scan(&items); err != nil {
			return nil, false, err
		}
		rfid_list = append(rfid_list, items)
	}

	if err := rows.Err(); err != nil {
		return nil, false, err
	}

	if len(rfid_list) == 0 {
		return nil, false, err
	}

	return rfid_list, true, nil
}

func InsertSingleRowToLogTable(db *sql.DB, rfid string, mode string) (bool, string, string, error) {
	log.Printf("Importing data to drfid_taglog table")
	Shop_code := utils.LoadConfig(utils.ADDRESS).ShopCode
	create_date := time.Now().Format(utils.TIME_FORMAT)
	query := `INSERT INTO drfid_taglog(dt_create_date, dt_com_shop_cd, dt_rfid_cd, dt_mode, sync_flag) VALUES (?, ?, ?, ?, 0);`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return false, create_date, Shop_code, err
	}
	defer stmt.Close()
	res, err := stmt.ExecContext(ctx, create_date, Shop_code, rfid, mode)
	if err != nil {
		log.Printf("Error %s when inserting row into log table", err)
		return false, create_date, Shop_code, err
	}
	rows, err := res.RowsAffected()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, create_date, Shop_code, err
	}
	log.Printf("%d rows created ", rows)
	return true, create_date, Shop_code, err
}

func InsertMaster(db *sql.DB,
	drgm_pos_shop_cd string,
	rf_goods_type string,
	rf_goods_cd_type string,
	drgm_rfid_cd string,
	drgm_jan string,
	drgm_jan2 string,
	drgm_goods_name string,
	drgm_goods_name_kana string,
	drgm_artist string,
	drgm_artist_kana string,
	drgm_maker_cd string,
	drgm_maker_name string,
	drgm_genre_cd string,
	drgm_maker_name_kana string,
	drgm_c_code string,
	drgm_selling_date string,
	drgm_price_tax_off int,
	drgm_cost_rate float64,
	drgm_cost_price int,
	drgm_media_cd string) (string, string, error) {

	log.Printf("Importing data to drfid_rfgoods_master table")
	Shop_code := utils.LoadConfig(utils.ADDRESS).ShopCode
	create_date := time.Now().Format(utils.DATE_FORMAT)
	query := `INSERT INTO drfid_rfgoods_master(
		drgm_create, 
		drgm_pos_shop_cd, 
		drgm_com_shop_cd, 
		rf_goods_type, 
		rf_goods_cd_type, 
		drgm_rfid_cd, 
		drgm_jan, 
		drgm_jan2, 
		drgm_goods_name, 
		drgm_goods_name_kana, 
		drgm_artist, 
		drgm_artist_kana, 
		drgm_maker_cd, 
		drgm_maker_name, 
		drgm_genre_cd, 
		drgm_maker_name_kana, 
		drgm_c_code, 
		drgm_selling_date, 
		drgm_price_tax_off, 
		drgm_cost_rate, 
		drgm_cost_price, 
		drgm_media_cd ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return create_date, Shop_code, err
	}
	defer stmt.Close()
	res, err := stmt.ExecContext(ctx,
		create_date,
		drgm_pos_shop_cd,
		Shop_code,
		rf_goods_type,
		rf_goods_cd_type,
		drgm_rfid_cd,
		drgm_jan,
		drgm_jan2,
		drgm_goods_name,
		drgm_goods_name_kana,
		drgm_artist,
		drgm_artist_kana,
		drgm_maker_cd,
		drgm_maker_name,
		drgm_genre_cd,
		drgm_maker_name_kana,
		drgm_c_code,
		drgm_selling_date,
		drgm_price_tax_off,
		drgm_cost_rate,
		drgm_cost_price,
		drgm_media_cd)
	if err != nil {
		log.Printf("Error %s when inserting row into master table", err)
		return create_date, Shop_code, err
	}
	rows, err := res.RowsAffected()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return create_date, Shop_code, err
	}
	log.Printf("%d rows created ", rows)
	return create_date, Shop_code, err
}

func DeleteMaster(db *sql.DB, rfid_list []string) (bool, error) {

	args := make([]interface{}, len(rfid_list))
	for i, id := range rfid_list {
		args[i] = id
	}

	query := `DELETE FROM drfid_rfgoods_master WHERE drgm_rfid_cd IN (?` + strings.Repeat(",?", len(args)-1) + `)`
	log.Printf("Deleting data from drfid_rfgoods_master table")

	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()

	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		log.Fatal(err)
	}

	_, err = tx.ExecContext(ctx, query, args...)
	if err != nil {
		// Incase we find any error in the query execution, rollback the transaction
		log.Printf("Error %s when finding rows affected", err)
		tx.Rollback()
		return false, err
	}
	err = tx.Commit()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}

	return true, err
}

func UpdateMaster(db *sql.DB,
	drgm_pos_shop_cd string,
	rf_goods_type string,
	rf_goods_cd_type string,
	drgm_rfid_cd string,
	drgm_jan string,
	drgm_jan2 string,
	drgm_goods_name string,
	drgm_goods_name_kana string,
	drgm_artist string,
	drgm_artist_kana string,
	drgm_maker_cd string,
	drgm_maker_name string,
	drgm_genre_cd string,
	drgm_maker_name_kana string,
	drgm_c_code string,
	drgm_selling_date string,
	drgm_price_tax_off int,
	drgm_cost_rate float64,
	drgm_cost_price int,
	drgm_media_cd string) (string, string, error) {

	log.Printf("Updating data in drfid_rfgoods_master table")
	Shop_code := utils.LoadConfig(utils.ADDRESS).ShopCode
	create_date := time.Now().Format(utils.DATE_FORMAT)
	query := `UPDATE drfid_rfgoods_master
		SET drgm_create = ?,
		drgm_pos_shop_cd = ?,
		drgm_com_shop_cd = ?,
		rf_goods_type = ?,
		rf_goods_cd_type = ?,
		drgm_rfid_cd = ?,
		drgm_jan = ?,
		drgm_jan2 = ?,
		drgm_goods_name = ?,
		drgm_goods_name_kana = ?,
		drgm_artist = ?,
		drgm_artist_kana = ?,
		drgm_maker_cd = ?,
		drgm_maker_name = ?,
		drgm_genre_cd = ?,
		drgm_maker_name_kana = ?,
		drgm_c_code = ?,
		drgm_selling_date = ?,
		drgm_price_tax_off = ?,
		drgm_cost_rate = ?,
		drgm_cost_price = ?,
		drgm_media_cd = ?
		WHERE drgm_rfid_cd = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return create_date, Shop_code, err
	}
	defer stmt.Close()
	res, err := stmt.ExecContext(ctx,
		create_date,
		drgm_pos_shop_cd,
		Shop_code,
		rf_goods_type,
		rf_goods_cd_type,
		drgm_rfid_cd,
		drgm_jan,
		drgm_jan2,
		drgm_goods_name,
		drgm_goods_name_kana,
		drgm_artist,
		drgm_artist_kana,
		drgm_maker_cd,
		drgm_maker_name,
		drgm_genre_cd,
		drgm_maker_name_kana,
		drgm_c_code,
		drgm_selling_date,
		drgm_price_tax_off,
		drgm_cost_rate,
		drgm_cost_price,
		drgm_media_cd,
		drgm_rfid_cd)
	if err != nil {
		log.Printf("Error %s when inserting row into master table", err)
		return create_date, Shop_code, err
	}
	rows, err := res.RowsAffected()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return create_date, Shop_code, err
	}
	log.Printf("%d rows created ", rows)
	return create_date, Shop_code, err
}

func SearchFromJan(db *sql.DB, jancode string) ([]Data_Search, bool, error) {
	log.Printf("Getting list of goods names from Jan code 1")
	query := `select 
	drgm_create, 
	drgm_pos_shop_cd, 
	drgm_com_shop_cd, 
	rf_goods_type, 
	rf_goods_cd_type, 
	drgm_rfid_cd, 
	drgm_jan, 
	drgm_jan2, 
	drgm_goods_name, 
	drgm_goods_name_kana, 
	drgm_artist, 
	drgm_artist_kana, 
	drgm_maker_cd, 
	drgm_maker_name, 
	drgm_genre_cd, 
	drgm_maker_name_kana, 
	drgm_c_code, 
	drgm_selling_date, 
	drgm_price_tax_off, 
	drgm_cost_rate, 
	drgm_cost_price, 
	drgm_media_cd 
	from drfid_rfgoods_master 
	where drgm_jan LIKE ? 
	group by drgm_jan2, drgm_goods_name;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return nil, false, err
	}
	defer stmt.Close()
	rows, err := stmt.QueryContext(ctx, jancode+"%")
	if err != nil {
		return nil, false, err
	}
	defer rows.Close()

	var data_list = []Data_Search{}
	for rows.Next() {
		var prd Data_Search
		if err := rows.Scan(&prd.Drgm_create,
			&prd.Drgm_pos_shop_cd,
			&prd.Drgm_com_shop_cd,
			&prd.Rf_goods_type,
			&prd.Rf_goods_cd_type,
			&prd.Drgm_rfid_cd,
			&prd.Drgm_jan,
			&prd.Drgm_jan2,
			&prd.Drgm_goods_name,
			&prd.Drgm_goods_name_kana,
			&prd.Drgm_artist,
			&prd.Drgm_artist_kana,
			&prd.Drgm_maker_cd,
			&prd.Drgm_maker_name,
			&prd.Drgm_genre_cd,
			&prd.Drgm_maker_name_kana,
			&prd.Drgm_c_code,
			&prd.Drgm_selling_date,
			&prd.Drgm_price_tax_off,
			&prd.Drgm_cost_rate,
			&prd.Drgm_cost_price,
			&prd.Drgm_media_cd); err != nil {
			log.Println(err)
			return []Data_Search{}, false, err
		}
		data_list = append(data_list, prd)
	}
	if err := rows.Err(); err != nil {
		return []Data_Search{}, false, err
	}

	if len(data_list) == 0 {
		query := `select 
		drgm_create,
		drgm_pos_shop_cd, 
		drgm_com_shop_cd, 
		rf_goods_type, 
		rf_goods_cd_type, 
		drgm_rfid_cd, 
		drgm_jan, 
		drgm_jan2, 
		drgm_goods_name, 
		drgm_goods_name_kana, 
		drgm_artist, 
		drgm_artist_kana, 
		drgm_maker_cd, 
		drgm_maker_name, 
		drgm_genre_cd, 
		drgm_maker_name_kana, 
		drgm_c_code, 
		drgm_selling_date, 
		drgm_price_tax_off, 
		drgm_cost_rate, 
		drgm_cost_price, 
		drgm_media_cd 
		from drfid_rfgoods_master 
		where drgm_jan2 LIKE ? 
		group by drgm_jan, drgm_goods_name;`

		ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
		defer cancelfunc()
		stmt, err := db.PrepareContext(ctx, query)
		if err != nil {
			log.Printf("Error %s when preparing SQL statement", err)
			return nil, false, err
		}
		defer stmt.Close()
		rows, err := stmt.QueryContext(ctx, jancode+"%")
		if err != nil {
			return nil, false, err
		}
		defer rows.Close()

		for rows.Next() {
			var prd Data_Search
			if err := rows.Scan(&prd.Drgm_create,
				&prd.Drgm_pos_shop_cd,
				&prd.Drgm_com_shop_cd,
				&prd.Rf_goods_type,
				&prd.Rf_goods_cd_type,
				&prd.Drgm_rfid_cd,
				&prd.Drgm_jan,
				&prd.Drgm_jan2,
				&prd.Drgm_goods_name,
				&prd.Drgm_goods_name_kana,
				&prd.Drgm_artist,
				&prd.Drgm_artist_kana,
				&prd.Drgm_maker_cd,
				&prd.Drgm_maker_name,
				&prd.Drgm_genre_cd,
				&prd.Drgm_maker_name_kana,
				&prd.Drgm_c_code,
				&prd.Drgm_selling_date,
				&prd.Drgm_price_tax_off,
				&prd.Drgm_cost_rate,
				&prd.Drgm_cost_price,
				&prd.Drgm_media_cd); err != nil {
				log.Println(err)
				return []Data_Search{}, false, err
			}
			data_list = append(data_list, prd)
		}
		if err := rows.Err(); err != nil {
			return []Data_Search{}, false, err
		}

	}
	if len(data_list) == 0 {
		return nil, false, err
	}

	return data_list, true, nil
}

func SearchFromJan_BQ(db *sql.DB, jancode string) (Data_Search, bool, error) {
	log.Printf("Getting information from Jancode")

	query := `select 
	bqsg_create_date, 
	bqsg_goods_type, 
	bqsg_jan_cd, 
	bqsg_goods_name, 
	bqsg_artist_name, 
	bqsg_artist_kana, 
	bqsg_publisher_cd, 
	bqsg_publisher_name, 
	bqsg_genre_cd, 
	bqsg_c_code, 
	bqsg_price_taxoff,
	bqsg_cost_price, 
	bqsg_media_cd,
	bqsg_shop_goods_price,
	bqsg_shop_goods_price_intax
	from bq_shop_goods_master
	where bqsg_jan_cd = ?`

	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return Data_Search{}, false, err
	}
	defer stmt.Close()
	rows, err := stmt.QueryContext(ctx, jancode)
	if err != nil {
		return Data_Search{}, false, err
	}

	defer rows.Close()

	var data_list = []Data_Search{}

	for rows.Next() {
		var prd Data_Search
		if err := rows.Scan(
			&prd.Drgm_create,
			&prd.Rf_goods_type,
			&prd.Drgm_jan,
			&prd.Drgm_goods_name,
			&prd.Drgm_artist,
			&prd.Drgm_artist_kana,
			&prd.Drgm_maker_cd,
			&prd.Drgm_maker_name,
			&prd.Drgm_genre_cd,
			&prd.Drgm_c_code,
			&prd.Drgm_price_tax_off,
			&prd.Drgm_cost_price,
			&prd.Drgm_media_cd,
			&prd.Bqsg_shop_goods_price,
			&prd.Bqsg_shop_goods_price_intax); err != nil {
			log.Println(err)
			return Data_Search{}, false, err
		}
		data_list = append(data_list, prd)
	}
	if err := rows.Err(); err != nil {
		return Data_Search{}, false, err
	}

	if len(data_list) == 0 {
		return Data_Search{}, false, err
	}
	return data_list[0], true, nil
}

func SearchFromName(db *sql.DB, names string) ([]Data_Search, bool, error) {
	log.Printf("Getting list of goods names from names")
	query := `select 
	drgm_create,
	drgm_pos_shop_cd, 
	drgm_com_shop_cd, 
	rf_goods_type, 
	rf_goods_cd_type, 
	drgm_rfid_cd, 
	drgm_jan, 
	drgm_jan2, 
	drgm_goods_name, 
	drgm_goods_name_kana, 
	drgm_artist, 
	drgm_artist_kana, 
	drgm_maker_cd, 
	drgm_maker_name, 
	drgm_genre_cd, 
	drgm_maker_name_kana, 
	drgm_c_code, 
	drgm_selling_date, 
	drgm_price_tax_off, 
	drgm_cost_rate, 
	drgm_cost_price, 
	drgm_media_cd 
	from drfid_rfgoods_master 
	where drgm_goods_name LIKE ? 
	group by drgm_jan, drgm_jan2;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return nil, false, err
	}
	defer stmt.Close()
	rows, err := stmt.QueryContext(ctx, "%"+names+"%")
	if err != nil {
		return nil, false, err
	}
	defer rows.Close()

	var data_list = []Data_Search{}

	for rows.Next() {
		var prd Data_Search
		if err := rows.Scan(&prd.Drgm_create,
			&prd.Drgm_pos_shop_cd,
			&prd.Drgm_com_shop_cd,
			&prd.Rf_goods_type,
			&prd.Rf_goods_cd_type,
			&prd.Drgm_rfid_cd,
			&prd.Drgm_jan,
			&prd.Drgm_jan2,
			&prd.Drgm_goods_name,
			&prd.Drgm_goods_name_kana,
			&prd.Drgm_artist,
			&prd.Drgm_artist_kana,
			&prd.Drgm_maker_cd,
			&prd.Drgm_maker_name,
			&prd.Drgm_genre_cd,
			&prd.Drgm_maker_name_kana,
			&prd.Drgm_c_code,
			&prd.Drgm_selling_date,
			&prd.Drgm_price_tax_off,
			&prd.Drgm_cost_rate,
			&prd.Drgm_cost_price,
			&prd.Drgm_media_cd); err != nil {
			log.Println(err)
			return []Data_Search{}, false, err
		}
		data_list = append(data_list, prd)
	}

	if err := rows.Err(); err != nil {
		return data_list, false, err
	}
	if len(data_list) == 0 {
		return nil, false, err
	}

	return data_list, true, nil
}

func GetGateSetting(db *sql.DB) (Gate_Setting, bool, error) {
	log.Printf("Getting setting of gate")
	query := `select
	use_gate_checkpoint,
	url_gate_checkpoint,
	username_gate,
	password_gate
	from rfid_app_setting;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return Gate_Setting{}, false, err
	}
	defer stmt.Close()
	var data = Gate_Setting{}

	row := stmt.QueryRowContext(ctx)
	if err := row.Scan(&data.UGC, &data.IP, &data.Username_gate, &data.Password_gate); err != nil {

		return Gate_Setting{}, false, err
	}
	return data, true, nil

}

func SetGateSetting(db *sql.DB, isUse int, IP string, username_gate string, password_gate string) (bool, error) {
	log.Printf("Getting setting of gate")

	if data, isSuccess, _ := GetGateSetting(db); isSuccess {
		if isUse != 0 && isUse != 1 {
			isUse = *data.UGC
		}
		if IP == "" {
			IP = *data.IP
		}
	}

	query := `UPDATE rfid_app_setting
		SET use_gate_checkpoint = ?,
		url_gate_checkpoint = ?,
		username_gate = ?,
		password_gate = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return false, err
	}
	defer stmt.Close()

	res, err := stmt.ExecContext(ctx, isUse, IP, username_gate, password_gate)
	if err != nil {
		log.Printf("Error %s when inserting row into setting table", err)
		return false, err
	}
	rows, err := res.RowsAffected()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}
	log.Printf("%d rows created ", rows)
	return true, err
}

func InsertTransaction_func(db *sql.DB,
	drtr_receipt_no string,
	drtr_shop_no string,
	drtr_pos_no string,
	drtr_date_create string,
	drtr_start_time string,
	drtr_end_time string,
	drtr_is_saved string) (bool, error) {

	log.Printf("Importing data to drfid_transaction table")
	query := `INSERT INTO drfid_transaction(
		drtr_receipt_no, 
		drtr_shop_no,  
		drtr_pos_no, 
		drtr_date_create, 
		drtr_start_time, 
		drtr_end_time, 
		drtr_is_saved) VALUES (?, ?, ?, ?, ?, ?, ?);`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return false, err
	}
	defer stmt.Close()
	res, err := stmt.ExecContext(ctx,
		drtr_receipt_no,
		drtr_shop_no,
		drtr_pos_no,
		drtr_date_create,
		drtr_start_time,
		drtr_end_time,
		drtr_is_saved)
	if err != nil {
		log.Printf("Error %s when inserting row into master table", err)
		return false, err
	}
	rows, err := res.RowsAffected()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}
	log.Printf("%d rows created ", rows)
	return true, nil
}

func UpdateTransactions(db *sql.DB, drtr_is_saved string, drtr_receipt_no string) (bool, error) {
	log.Printf("Getting setting of gate")

	query := `UPDATE drfid_transaction
		SET drtr_is_saved = ?
		WHERE drtr_receipt_no = ? `
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return false, err
	}
	defer stmt.Close()

	res, err := stmt.ExecContext(ctx, drtr_is_saved, drtr_receipt_no)
	if err != nil {
		log.Printf("Error %s when inserting row into setting table", err)
		return false, err
	}
	rows, err := res.RowsAffected()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}
	log.Printf("%d rows created ", rows)
	return true, err
}

func Get_not_saved_transaction_func_OLD(db *sql.DB) ([]InsertTransaction, bool, error) {
	log.Printf("Getting setting of gate")
	query := `select
	*
	from drfid_transaction
	where drtr_is_saved = "0"  ;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return []InsertTransaction{}, false, err
	}
	defer stmt.Close()
	var data = InsertTransaction{}
	result := []InsertTransaction{}

	row := stmt.QueryRowContext(ctx)
	if err := row.Scan(&data.Drtr_receipt_no, &data.Drtr_shop_no, &data.Drtr_pos_no, &data.Drtr_date_create, &data.Drtr_start_time, &data.Drtr_end_time, &data.Drtr_is_saved); err != nil {

		return []InsertTransaction{}, false, err
	}
	result = append(result, data)
	return result, true, nil

}

func Get_not_saved_transaction_func(db *sql.DB) ([]InsertTransaction, bool, error) {
	log.Printf("Getting list transactions")
	query := `select
	*
	from drfid_transaction
	where drtr_is_saved = "0" 
	ORDER BY drtr_date_create DESC, drtr_end_time DESC;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return nil, false, err
	}
	defer stmt.Close()
	rows, err := stmt.QueryContext(ctx)
	if err != nil {
		return nil, false, err
	}
	defer rows.Close()

	var data_list = []InsertTransaction{}
	for rows.Next() {
		var data InsertTransaction
		if err := rows.Scan(&data.Drtr_receipt_no, &data.Drtr_shop_no, &data.Drtr_pos_no, &data.Drtr_date_create, &data.Drtr_start_time, &data.Drtr_end_time, &data.Drtr_is_saved); err != nil {
			log.Println(err)
			return []InsertTransaction{}, false, err
		}
		data_list = append(data_list, data)
	}
	if err := rows.Err(); err != nil {
		return []InsertTransaction{}, false, err
	}

	return data_list, true, nil
}
