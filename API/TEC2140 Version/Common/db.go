package Common

import (
	"database/sql"
	"fmt"
	"strconv"

	"ProductManage/github.com/goframework/gcp/bq"

	_ "github.com/go-sql-driver/mysql"
	"github.com/goframework/gf/cfg"
	"github.com/goframework/gf/db"
	"github.com/goframework/gf/exterror"
)

type DBConnection struct {
	Driver string
	Host   string
	Port   string
	User   string
	Pwd    string
	DBName string

	DB *sql.DB
}

func NewDatabase(config cfg.Cfg) DBConnection {
	db := DBConnection{}
	db.Driver = config.StrOrEmpty(CfgDBDriver)
	db.Host = config.StrOrEmpty(CfgDBHost)
	db.Port = config.StrOrEmpty(CfgDBPort)
	db.User = config.StrOrEmpty(CfgDBUser)
	db.Pwd = config.StrOrEmpty(CfgDBPwd)
	db.DBName = config.StrOrEmpty(CfgDBName)
	fmt.Println(db)
	return db
}

func (this *DBConnection) Connect() error {
	db, err := sql.Open(this.Driver, this.User+":"+this.Pwd+"@tcp("+this.Host+":"+this.Port+")/"+this.DBName)
	if err != nil {
		return exterror.WrapExtError(err)
	}
	this.DB = db
	return nil
}

func (this *DBConnection) Close() {
	if this.DB != nil {
		this.DB.Close()
	}
}

type TableInfo struct {
	UpdateTime string `sql:"Update_time"`
}

type Drfid_selfregi_video struct {
	ApiKey             string `json:"api_key"`
	Drsv_shop_cd       string `json:"drsv_shop_cd" sql:"drsv_shop_cd"`
	Drsv_pos_no        string `json:"drsv_pos_no" sql:"drsv_pos_no"`
	Drsv_receipt_no    string `json:"drsv_receipt_no" sql:"drsv_receipt_no"`
	Drsv_date          string `json:"drsv_date" sql:"drsv_date"`
	Drsv_start_time    string `json:"drsv_start_time" sql:"drsv_start_time"`
	Drsv_end_time      string `json:"drsv_end_time" sql:"drsv_end_time"`
	Drsv_customer_base string `json:"drsv_customer_base" sql:"drsv_customer_base"`
	Drsv_video_link    string `json:"drsv_video_link" sql:"drsv_video_link"`
	Drsv_thumnail      string `json:"drsv_thumnail" sql:"drsv_thumnail"`
}

func GetTableUpdateTime(tableID string) (string, *ErrorDetail) {
	conn := NewDatabase(GConfig)
	connErr := conn.Connect()
	if connErr != nil {
		return "", &ErrorDetail{
			exterror.TraceError(connErr),
			FAIL_CAUSE_DB_CONNECT,
		}
	}

	defer conn.Close()

	query := `SHOW TABLE STATUS LIKE '` + tableID + `'`

	rows, err := conn.DB.Query(query)
	if err != nil {
		return "", &ErrorDetail{
			exterror.TraceError(err),
			FAIL_CAUSE_DB_QUERY,
		}
	}
	defer rows.Close()

	if rows.Next() {
		ti := TableInfo{}
		err := db.SqlScanStruct(rows, &ti)
		if err != nil {
			return "", &ErrorDetail{
				exterror.TraceError(err),
				FAIL_CAUSE_DB_DATA_GET,
			}
		} else {
			return ti.UpdateTime, nil
		}
	}

	return "", nil
}

func InsertSelfRegiBQ(logs Drfid_selfregi_video) error {
	bqc, err := GetBQConnection()
	if err != nil {
		fmt.Println(err)
		return exterror.WrapExtError(err)
	}

	cmd := bq.NewCommand()
	cmd.CommandText = `
	#standardSQL
	INSERT {{@src_dataset}}.drfid_selfregi_video (drsv_shop_cd,drsv_pos_no, drsv_receipt_no, drsv_date, drsv_start_time, drsv_end_time, drsv_customer_base, drsv_video_link, drsv_thumnail)
	VALUES`
	// Add values to command text
	// for i, _ := range logs {
	// if i == len(logs)-1 {
	cmd.CommandText = cmd.CommandText +
		`("` + logs.Drsv_shop_cd +
		`","` + logs.Drsv_pos_no +
		`","` + logs.Drsv_receipt_no +
		`","` + logs.Drsv_date +
		`","` + logs.Drsv_start_time +
		`","` + logs.Drsv_end_time +
		`","` + logs.Drsv_customer_base +
		`","` + logs.Drsv_video_link +
		`","` + logs.Drsv_thumnail +
		`")`
		// 	break
		// }
		//cmd.CommandText = cmd.CommandText + `('` + logs[i].dtCreateDate.String + `','` + logs[i].dtComShopCd.String + `','` + logs[i].dtRfidCd.String + `','` + logs[i].dtMode.String + `'),`
	// }

	cmd.Parameters["@src_dataset"] = "RF_Data_test"

	query, err := cmd.Build()
	if err != nil {
		fmt.Println(err)
		return exterror.WrapExtError(err)
	}

	_, _, err = bqc.QueryForResponseBySql(query, nil, "drfid")
	if err != nil {
		fmt.Println(err)
		return exterror.WrapExtError(err)
	}

	return nil

}

func GetTotalRowsSelfRegiBQ() (int, error) {
	cmd := bq.NewCommand()
	cmd.CommandText = `
#standardsql
SELECT * FROM {{@src_dataset}}.drfid_selfregi_video`

	cmd.Parameters["@src_dataset"] = "RF_Data_test"
	query, err := cmd.Build()
	if err != nil {
		return 0, exterror.WrapExtError(err)
	}

	bqc, err := GetBQConnection()
	if err != nil {
		return 0, exterror.WrapExtError(err)
	}

	totalRows, _, err := bqc.QueryForResponseBySql(query, nil, "drfid")
	if err != nil {
		return 0, exterror.WrapExtError(err)
	}

	return int(totalRows), nil
}

func GetDataSelfRegiBQ(page int64, rows_int int64) ([]*Drfid_selfregi_video, error) {
	offset_int := (page - 1) * rows_int
	offset := strconv.FormatInt(offset_int, 10)
	rows := strconv.FormatInt(rows_int, 10)
	cmd := bq.NewCommand()
	cmd.CommandText = `
#standardsql
SELECT * FROM {{@src_dataset}}.drfid_selfregi_video LIMIT ` + rows + ` OFFSET ` + offset

	cmd.Parameters["@src_dataset"] = "RF_Data_test"
	query, err := cmd.Build()
	if err != nil {
		return nil, exterror.WrapExtError(err)
	}

	bqc, err := GetBQConnection()
	if err != nil {
		return nil, exterror.WrapExtError(err)
	}

	totalRows, jobID, err := bqc.QueryForResponseBySql(query, nil, "drfid")
	if err != nil {
		return nil, exterror.WrapExtError(err)
	}

	var products []*Drfid_selfregi_video
	dataChan, _ := bqc.GetResponseData(jobID, 0, int(totalRows))
	for {
		row := <-dataChan
		if row == nil {
			break
		}
		r := Drfid_selfregi_video{}
		row.ToStruct(&r)
		products = append(products, &r)
	}

	return products, nil
}

func GetDataSelfRegiBQ_NEW(page int64,
	rows_int int64,
	drsv_shop_cd string,
	drsv_pos_no_from string,
	drsv_pos_no_to string,

	drsv_receipt_no_from string,
	drsv_receipt_no_to string,

	drsv_date_from string,
	drsv_date_to string,

	drsv_start_time string,
	drsv_end_time string,
	drsv_customer_base_from string,
	drsv_customer_base_to string) ([]*Drfid_selfregi_video, error) {
	offset_int := (page - 1) * rows_int
	offset := strconv.FormatInt(offset_int, 10)
	rows := strconv.FormatInt(rows_int, 10)
	var sql_1 string
	var sql_2 string
	var sql_3 string
	var sql_4 string

	if drsv_pos_no_from != "" && drsv_pos_no_to != "" {
		sql_1 = `AND (( "` + drsv_pos_no_from + `" <> "" AND drsv_pos_no BETWEEN "` + drsv_pos_no_from + `" AND "` + drsv_pos_no_to + `" ) OR ( "` + drsv_pos_no_from + `" = "")) `
	} else if drsv_pos_no_from != "" && drsv_pos_no_to == "" {
		sql_1 = `AND (( "` + drsv_pos_no_from + `" <> "" AND drsv_pos_no >= "` + drsv_pos_no_from + `") OR ( "` + drsv_pos_no_from + `" = "")) `
	} else if drsv_pos_no_from == "" && drsv_pos_no_to != "" {
		sql_1 = `AND (( "` + drsv_pos_no_to + `" <> "" AND drsv_pos_no <= "` + drsv_pos_no_to + `") OR ( "` + drsv_pos_no_to + `" = "")) `
	} else {
		sql_1 = ``
	}

	if drsv_receipt_no_from != "" && drsv_receipt_no_to != "" {
		sql_2 = `AND (( "` + drsv_receipt_no_from + `" <> "" AND drsv_receipt_no BETWEEN "` + drsv_receipt_no_from + `" AND "` + drsv_receipt_no_to + `" ) OR ( "` + drsv_receipt_no_from + `" = "")) `
	} else if drsv_receipt_no_from != "" && drsv_receipt_no_to == "" {
		sql_2 = `AND (( "` + drsv_receipt_no_from + `" <> "" AND drsv_receipt_no >= "` + drsv_receipt_no_from + `") OR ( "` + drsv_receipt_no_from + `" = "")) `
	} else if drsv_receipt_no_from == "" && drsv_receipt_no_to != "" {
		sql_2 = `AND (( "` + drsv_receipt_no_to + `" <> "" AND drsv_receipt_no <= "` + drsv_receipt_no_to + `") OR ( "` + drsv_receipt_no_to + `" = "")) `
	} else {
		sql_2 = ``
	}

	if drsv_date_from != "" && drsv_date_to != "" {
		sql_3 = `AND (( "` + drsv_date_from + `" <> "" AND drsv_date BETWEEN "` + drsv_date_from + `" AND "` + drsv_date_to + `" ) OR ( "` + drsv_date_from + `" = "")) `
	} else if drsv_date_from != "" && drsv_date_to == "" {
		sql_3 = `AND (( "` + drsv_date_from + `" <> "" AND drsv_date >= "` + drsv_date_from + `") OR ( "` + drsv_date_from + `" = "")) `
	} else if drsv_date_from == "" && drsv_date_to != "" {
		sql_3 = `AND (( "` + drsv_date_to + `" <> "" AND drsv_date <= "` + drsv_date_to + `") OR ( "` + drsv_date_to + `" = "")) `
	} else {
		sql_3 = ``
	}

	if drsv_customer_base_from != "" && drsv_customer_base_to != "" {
		sql_4 = `AND (( "` + drsv_customer_base_from + `" <> "" AND drsv_customer_base BETWEEN "` + drsv_customer_base_from + `" AND "` + drsv_customer_base_to + `" ) OR ( "` + drsv_customer_base_from + `" = "")) `
	} else if drsv_customer_base_from != "" && drsv_customer_base_to == "" {
		sql_4 = `AND (( "` + drsv_customer_base_from + `" <> "" AND drsv_customer_base >= "` + drsv_customer_base_from + `") OR ( "` + drsv_customer_base_from + `" = "")) `
	} else if drsv_customer_base_from == "" && drsv_customer_base_to != "" {
		sql_4 = `AND (( "` + drsv_customer_base_to + `" <> "" AND drsv_customer_base <= "` + drsv_customer_base_to + `") OR ( "` + drsv_customer_base_to + `" = "")) `
	} else {
		sql_4 = ``
	}

	cmd := bq.NewCommand()
	cmd.CommandText = `
	#standardsql
	SELECT * FROM {{@src_dataset}}.drfid_selfregi_video WHERE 1 = 1 
	AND (( "` + drsv_shop_cd + `" <> "" AND drsv_shop_cd = "` + drsv_shop_cd + `" ) OR ( "` + drsv_shop_cd + `" = ""))
	` + sql_1 + sql_2 + sql_3 + sql_4 + `
	AND (( "` + drsv_start_time + `" <> "" AND drsv_start_time >= "` + drsv_start_time + `" ) OR ( "` + drsv_start_time + `" = "")) 
	AND (( "` + drsv_end_time + `" <> "" AND drsv_end_time <= "` + drsv_end_time + `" ) OR ( "` + drsv_end_time + `" = "")) 
	ORDER BY drsv_date DESC, drsv_start_time DESC LIMIT ` + rows + ` OFFSET ` + offset

	cmd.Parameters["@src_dataset"] = "RF_Data_test"
	query, err := cmd.Build()
	if err != nil {
		return nil, exterror.WrapExtError(err)
	}

	fmt.Println("1 la`", query)

	bqc, err := GetBQConnection()
	if err != nil {
		return nil, exterror.WrapExtError(err)
	}

	totalRows, jobID, err := bqc.QueryForResponseBySql(query, nil, "drfid")
	if err != nil {
		return nil, exterror.WrapExtError(err)
	}

	var products []*Drfid_selfregi_video
	dataChan, _ := bqc.GetResponseData(jobID, 0, int(totalRows))
	for {
		row := <-dataChan
		if row == nil {
			break
		}
		r := Drfid_selfregi_video{}
		row.ToStruct(&r)
		products = append(products, &r)
	}

	return products, nil
}

func GetDataSelfRegiBQ_NEW_Test(page int64,
	rows_int int64,
	drsv_shop_cd string,
	drsv_pos_no string,
	drsv_receipt_no string,
	drsv_date string,
	drsv_start_time string,
	drsv_end_time string,
	drsv_customer_base string) ([]*Drfid_selfregi_video, error) {
	offset_int := (page - 1) * rows_int
	offset := strconv.FormatInt(offset_int, 10)
	rows := strconv.FormatInt(rows_int, 10)
	cmd := bq.NewCommand()

	cmd.CommandText = `
#standardsql
SELECT * FROM {{@src_dataset}}.drfid_selfregi_video WHERE 1 = 1 
AND (( "` + drsv_shop_cd + `" <> "" AND drsv_shop_cd = "` + drsv_shop_cd + `" ) OR ( "` + drsv_shop_cd + `" = ""))
AND (( "` + drsv_pos_no + `" <> "" AND drsv_pos_no = "` + drsv_pos_no + `" ) OR ( "` + drsv_pos_no + `" = "")) 
AND (( "` + drsv_receipt_no + `" <> "" AND drsv_receipt_no = "` + drsv_receipt_no + `" ) OR ( "` + drsv_receipt_no + `" = "")) 
AND (( "` + drsv_date + `" <> "" AND drsv_date = "` + drsv_date + `" ) OR ( "` + drsv_date + `" = "")) 
AND (( "` + drsv_start_time + `" <> "" AND drsv_start_time >= "` + drsv_start_time + `" ) OR ( "` + drsv_start_time + `" = "")) 
AND (( "` + drsv_end_time + `" <> "" AND drsv_end_time <= "` + drsv_end_time + `" ) OR ( "` + drsv_end_time + `" = "")) 
AND (( "` + drsv_customer_base + `" <> "" AND drsv_customer_base = "` + drsv_customer_base + `" ) OR ( "` + drsv_customer_base + `" = "")) 

ORDER BY drsv_date, drsv_start_time DESC LIMIT ` + rows + ` OFFSET ` + offset

	cmd.Parameters["@src_dataset"] = "RF_Data_test"
	query, err := cmd.Build()
	if err != nil {
		return nil, exterror.WrapExtError(err)
	}

	bqc, err := GetBQConnection()
	if err != nil {
		return nil, exterror.WrapExtError(err)
	}

	totalRows, jobID, err := bqc.QueryForResponseBySql(query, nil, "drfid")
	if err != nil {
		return nil, exterror.WrapExtError(err)
	}

	var products []*Drfid_selfregi_video
	dataChan, _ := bqc.GetResponseData(jobID, 0, int(totalRows))
	for {
		row := <-dataChan
		if row == nil {
			break
		}
		r := Drfid_selfregi_video{}
		row.ToStruct(&r)
		products = append(products, &r)
	}

	return products, nil
}

func GetTotalRowsSelfRegiBQ_New(
	drsv_shop_cd string,
	drsv_pos_no_from string,
	drsv_pos_no_to string,

	drsv_receipt_no_from string,
	drsv_receipt_no_to string,

	drsv_date_from string,
	drsv_date_to string,

	drsv_start_time string,
	drsv_end_time string,
	drsv_customer_base_from string,
	drsv_customer_base_to string) (int, error) {
	var sql_1 string
	var sql_2 string
	var sql_3 string
	var sql_4 string

	if drsv_pos_no_from != "" && drsv_pos_no_to != "" {
		sql_1 = `AND (( "` + drsv_pos_no_from + `" <> "" AND drsv_pos_no BETWEEN "` + drsv_pos_no_from + `" AND "` + drsv_pos_no_to + `" ) OR ( "` + drsv_pos_no_from + `" = "")) `
	} else if drsv_pos_no_from != "" && drsv_pos_no_to == "" {
		sql_1 = `AND (( "` + drsv_pos_no_from + `" <> "" AND drsv_pos_no >= "` + drsv_pos_no_from + `") OR ( "` + drsv_pos_no_from + `" = "")) `
	} else if drsv_pos_no_from == "" && drsv_pos_no_to != "" {
		sql_1 = `AND (( "` + drsv_pos_no_to + `" <> "" AND drsv_pos_no <= "` + drsv_pos_no_to + `") OR ( "` + drsv_pos_no_to + `" = "")) `
	} else {
		sql_1 = ``
	}

	if drsv_receipt_no_from != "" && drsv_receipt_no_to != "" {
		sql_2 = `AND (( "` + drsv_receipt_no_from + `" <> "" AND drsv_receipt_no BETWEEN "` + drsv_receipt_no_from + `" AND "` + drsv_receipt_no_to + `" ) OR ( "` + drsv_receipt_no_from + `" = "")) `
	} else if drsv_receipt_no_from != "" && drsv_receipt_no_to == "" {
		sql_2 = `AND (( "` + drsv_receipt_no_from + `" <> "" AND drsv_receipt_no >= "` + drsv_receipt_no_from + `") OR ( "` + drsv_receipt_no_from + `" = "")) `
	} else if drsv_receipt_no_from == "" && drsv_receipt_no_to != "" {
		sql_2 = `AND (( "` + drsv_receipt_no_to + `" <> "" AND drsv_receipt_no <= "` + drsv_receipt_no_to + `") OR ( "` + drsv_receipt_no_to + `" = "")) `
	} else {
		sql_2 = ``
	}

	if drsv_date_from != "" && drsv_date_to != "" {
		sql_3 = `AND (( "` + drsv_date_from + `" <> "" AND drsv_date BETWEEN "` + drsv_date_from + `" AND "` + drsv_date_to + `" ) OR ( "` + drsv_date_from + `" = "")) `
	} else if drsv_date_from != "" && drsv_date_to == "" {
		sql_3 = `AND (( "` + drsv_date_from + `" <> "" AND drsv_date >= "` + drsv_date_from + `") OR ( "` + drsv_date_from + `" = "")) `
	} else if drsv_date_from == "" && drsv_date_to != "" {
		sql_3 = `AND (( "` + drsv_date_to + `" <> "" AND drsv_date <= "` + drsv_date_to + `") OR ( "` + drsv_date_to + `" = "")) `
	} else {
		sql_3 = ``
	}

	if drsv_customer_base_from != "" && drsv_customer_base_to != "" {
		sql_4 = `AND (( "` + drsv_customer_base_from + `" <> "" AND drsv_customer_base BETWEEN "` + drsv_customer_base_from + `" AND "` + drsv_customer_base_to + `" ) OR ( "` + drsv_customer_base_from + `" = "")) `
	} else if drsv_customer_base_from != "" && drsv_customer_base_to == "" {
		sql_4 = `AND (( "` + drsv_customer_base_from + `" <> "" AND drsv_customer_base >= "` + drsv_customer_base_from + `") OR ( "` + drsv_customer_base_from + `" = "")) `
	} else if drsv_customer_base_from == "" && drsv_customer_base_to != "" {
		sql_4 = `AND (( "` + drsv_customer_base_to + `" <> "" AND drsv_customer_base <= "` + drsv_customer_base_to + `") OR ( "` + drsv_customer_base_to + `" = "")) `
	} else {
		sql_4 = ``
	}

	cmd := bq.NewCommand()
	cmd.CommandText = `
	#standardsql
	SELECT * FROM {{@src_dataset}}.drfid_selfregi_video WHERE 1 = 1 
	AND (( "` + drsv_shop_cd + `" <> "" AND drsv_shop_cd = "` + drsv_shop_cd + `" ) OR ( "` + drsv_shop_cd + `" = ""))
	` + sql_1 + sql_2 + sql_3 + sql_4 + `
	AND (( "` + drsv_start_time + `" <> "" AND drsv_start_time >= "` + drsv_start_time + `" ) OR ( "` + drsv_start_time + `" = "")) 
	AND (( "` + drsv_end_time + `" <> "" AND drsv_end_time <= "` + drsv_end_time + `" ) OR ( "` + drsv_end_time + `" = "")) 
	ORDER BY drsv_date, drsv_start_time DESC `
	cmd.Parameters["@src_dataset"] = "RF_Data_test"

	query, err := cmd.Build()
	if err != nil {
		return 0, exterror.WrapExtError(err)
	}

	bqc, err := GetBQConnection()
	if err != nil {
		return 0, exterror.WrapExtError(err)
	}

	totalRows, _, err := bqc.QueryForResponseBySql(query, nil, "drfid")
	if err != nil {
		return 0, exterror.WrapExtError(err)
	}

	return int(totalRows), nil
}
