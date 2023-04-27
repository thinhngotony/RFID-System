package controllers

import (
	"context"
	"database/sql"
	"io/ioutil"
	"log"
	"main/db_client"
	"main/utils"
	"net/http"
	"regexp"
	"strings"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/go-playground/validator"
)

type Auth_Test struct {
	Username_gate string `json:"username" validate:"required"`
	Password_gate string `json:"password" validate:"required"`
	Token         string `json:"accessToken"`
	TokenType     string `json:"tokenType"`
}

type GetBlackList struct {
	From string `json:"from"`
	To   string `json:"to"`
}

func CheckAuth(db *sql.DB, reqBody Auth_Test) bool {
	log.Printf("Getting thông tin token")
	query := `select username_gate, password_gate from rfid_app_setting where username_gate = ? AND password_gate = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return false
	}
	defer stmt.Close()
	var data = Auth_Test{}
	row := stmt.QueryRowContext(ctx, reqBody.Username_gate, reqBody.Password_gate)
	if err := row.Scan(&data.Username_gate, &data.Password_gate); err != nil {

		return false
	}

	if data.Username_gate == reqBody.Username_gate && data.Password_gate == reqBody.Password_gate {
		return true
	} else {
		return false
	}

}

func GetTokenByAuth(c *gin.Context) {
	var reqBody Auth_Test
	if err := c.ShouldBindJSON(&reqBody); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    "404",
			"data":    nil,
			"message": "jsonリクエストの本文の形式が正しくありません。",
		})
		return
	}
	if err := validator.New().Struct(reqBody); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    "401",
			"message": "Thiếu thông tin username hoặc password",
		})
		return
	}

	db, _ := db_client.DbConnection()
	defer db.Close()

	if isSuccess := CheckAuth(db, reqBody); isSuccess {
		c.JSON(http.StatusOK, gin.H{
			"accessToken": utils.ACCESS_TOKEN,
			"tokenType":   utils.TOKEN_TYPE,
		})
	} else {
		c.JSON(http.StatusOK, gin.H{
			"code": "201",
		})
	}
}

// DELETE
type RFID_Exist_Test struct {
	RFID_Exist     []string `json:"rfid_exist"`
	RFID_Not_Exist []string `json:"rfid_not_exist"`
}

type UnloadInventory_Test struct {
	UnloadList []string `binding:"required"`
}

type Header struct {
	Authorization *int `header:"Bearer" binding:"required"`
}

func CheckExistBlackList(db *sql.DB, unloadInventory []string) (RFID_Exist_Test, error) {
	log.Printf("Checking RFID in database ")
	query := `select rfid from black_list where rfid = ? ;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return RFID_Exist_Test{}, err
	}
	defer stmt.Close()

	var data = RFID_Exist_Test{}

	for _, rfid := range unloadInventory {
		rows, err := stmt.QueryContext(ctx, rfid)
		if err != nil {
			return RFID_Exist_Test{}, err
		}
		defer rows.Close()
		for rows.Next() {
			var data_exist string
			if err := rows.Scan(&data_exist); err != nil {
				return RFID_Exist_Test{}, err
			}
			data.RFID_Exist = append(data.RFID_Exist, data_exist)
		}

		if err := rows.Err(); err != nil {
			return RFID_Exist_Test{}, err
		}
	}

	data.RFID_Not_Exist = db_client.DifferenceSlice(unloadInventory, data.RFID_Exist)
	return data, nil
}

func DeleteMaster_Test(db *sql.DB, rfid_list []string) (bool, error) {
	args := make([]interface{}, len(rfid_list))
	for i, id := range rfid_list {
		args[i] = id
	}

	query := `DELETE FROM black_list WHERE rfid IN (?` + strings.Repeat(",?", len(args)-1) + `)`
	log.Printf("Deleting data from black_list table")

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

func ConvertStringtoSlice(data string) []string {
	var result []string
	re := regexp.MustCompile(`"[^"]*"`)
	newStrs := re.FindAllString(data, -1)
	for _, s := range newStrs {
		y := ConvertStringtoSlice_NEW(s)
		result = append(result, y)
	}
	return result
}

func ConvertStringtoSlice_NEW(data string) string {

	if data[0] == '"' {
		data = data[1:]
	}
	if i := len(data) - 1; data[i] == '"' {
		data = data[:i]
	}
	return data
}

func UnloadInventory(c *gin.Context) {
	token := strings.Split(c.Request.Header["Authorization"][0], " ")[1]

	if token != utils.ACCESS_TOKEN {
		c.JSON(http.StatusOK, gin.H{
			"code":           "401",
			"token_request":  token,
			"token_validate": utils.ACCESS_TOKEN,
			"message":        "Sai thông tin chứng thực",
		})
		return
	}

	var value string
	if data, err := ioutil.ReadAll(c.Request.Body); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    "401",
			"data":    data,
			"message": "Không thể đọc được data request",
		})
	} else {
		value = string(data[:])
	}

	value_slice := ConvertStringtoSlice(value)
	db, _ := db_client.DbConnection()
	defer db.Close()

	if data, err := CheckExistBlackList(db, value_slice); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code": "403",
		})
	} else if len(data.RFID_Not_Exist) > 0 {
		c.JSON(http.StatusOK, gin.H{
			"code":    "403",
			"message": "Trong mảng có RFID không tồn tại",
			"data":    data.RFID_Not_Exist,
		})
	} else {
		// Xử lí xóa
		if isSuccess, _ := DeleteMaster_Test(db, value_slice); isSuccess {
			c.JSON(http.StatusOK, gin.H{
				"code":    "200",
				"message": "Xóa thành công",
			})
		} else {
			c.JSON(http.StatusOK, gin.H{
				"code":    "201",
				"message": "Có lỗi xảy ra trong quá trình xóa",
			})
		}

	}
}

func InsertRFID_TEST(db *sql.DB, rfid string) (bool, error) {
	log.Printf("Importing data to black_list table")
	query := `INSERT INTO black_list(rfid, create_date) VALUES (?, ?);`
	create_date := time.Now().Format(utils.TIME_FORMAT)
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return false, err
	}
	defer stmt.Close()
	res, err := stmt.ExecContext(ctx, rfid, create_date)
	if err != nil {
		log.Printf("Error %s when inserting row into log table", err)
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

func Inventory(c *gin.Context) {
	token := strings.Split(c.Request.Header["Authorization"][0], " ")[1]

	if token != utils.ACCESS_TOKEN {
		c.JSON(http.StatusOK, gin.H{
			"code":           "401",
			"token_request":  token,
			"token_validate": utils.ACCESS_TOKEN,
			"message":        "Sai thông tin chứng thực",
		})
		return
	}

	var value string
	if data, err := ioutil.ReadAll(c.Request.Body); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    "401",
			"data":    data,
			"message": "Không thể đọc được data request",
		})
	} else {
		value = string(data[:])
	}

	value_slice := ConvertStringtoSlice(value)
	db, _ := db_client.DbConnection()
	defer db.Close()

	if data, err := CheckExistBlackList(db, value_slice); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code": "403",
		})
	} else if len(data.RFID_Exist) > 0 {
		c.JSON(http.StatusOK, gin.H{
			"code":    "403",
			"message": "Trong mảng có RFID đã tồn tại",
			"data":    data.RFID_Exist,
		})
	} else {
		// Xử lí insert
		var isSuccess bool
		for _, value := range value_slice {
			isSuccess, _ = InsertRFID_TEST(db, value)
		}

		if isSuccess {
			c.JSON(http.StatusOK, gin.H{
				"code":    "200",
				"message": "Thêm thành công",
			})
		} else {
			c.JSON(http.StatusOK, gin.H{
				"code":    "201",
				"message": "Có lỗi xảy ra trong quá trình thêm",
			})
		}

	}
}

func Delete_Test(db *sql.DB) (bool, error) {

	query := `TRUNCATE TABLE black_list`
	log.Printf("Deleting all data from block table")

	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()

	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		log.Fatal(err)
	}
	_, err = tx.ExecContext(ctx, query)
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

func Inventory_2(c *gin.Context) {
	token := strings.Split(c.Request.Header["Authorization"][0], " ")[1]

	if token != utils.ACCESS_TOKEN {
		c.JSON(http.StatusOK, gin.H{
			"code":           "401",
			"token_request":  token,
			"token_validate": utils.ACCESS_TOKEN,
			"message":        "Sai thông tin chứng thực",
		})
		return
	}

	var value string
	if data, err := ioutil.ReadAll(c.Request.Body); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    "401",
			"data":    data,
			"message": "Không thể đọc được data request",
		})
	} else {
		value = string(data[:])
	}

	value_slice := ConvertStringtoSlice(value)
	db, _ := db_client.DbConnection()
	defer db.Close()

	// Xử lí insert
	if _, err := Delete_Test(db); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    "201",
			"message": "Có lỗi xảy ra trong quá trình xóa",
		})
	} else {
		var isSuccess bool
		for _, value := range value_slice {
			isSuccess, _ = InsertRFID_TEST(db, value)
		}

		if isSuccess {
			c.JSON(http.StatusOK, gin.H{
				"code":    "200",
				"message": "Thêm thành công",
			})
		} else {
			c.JSON(http.StatusOK, gin.H{
				"code":    "201",
				"message": "Có lỗi xảy ra trong quá trình thêm",
			})
		}
	}

}

func Get_blacklist_db(db *sql.DB, from string, to string) ([]string, bool, error) {
	log.Printf("Select data from black_list table")
	var black_list_data []string
	if from != "" && to != "" {
		query := `select rfid from black_list where create_date >= ? AND create_date <= DATE_ADD(?,INTERVAL 1 DAY) order by create_date DESC;`
		ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
		defer cancelfunc()
		stmt, err := db.PrepareContext(ctx, query)
		if err != nil {
			log.Printf("Error %s when preparing SQL statement", err)
			return nil, false, err
		}
		defer stmt.Close()

		rows, err := stmt.QueryContext(ctx, from, to)
		if err != nil {
			return nil, false, err
		}
		defer rows.Close()

		for rows.Next() {
			var prd string
			if err := rows.Scan(&prd); err != nil {
				return nil, false, err
			}
			black_list_data = append(black_list_data, prd)
		}

		if err := rows.Err(); err != nil {
			return nil, false, err
		}

		if len(black_list_data) == 0 {
			return nil, false, err
		}
	} else {
		query := `select rfid from black_list;`
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

		for rows.Next() {
			var prd string
			if err := rows.Scan(&prd); err != nil {
				return nil, false, err
			}
			black_list_data = append(black_list_data, prd)
		}

		if err := rows.Err(); err != nil {
			return nil, false, err
		}

		if len(black_list_data) == 0 {
			return nil, false, err
		}
	}

	return black_list_data, true, nil
}

func Get_blacklist_db_NEW(db *sql.DB) ([]string, bool, error) {
	log.Printf("Select data from black_list table")
	var black_list_data []string
	query := `select rfid from black_list;`
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

	for rows.Next() {
		var prd string
		if err := rows.Scan(&prd); err != nil {
			return nil, false, err
		}
		black_list_data = append(black_list_data, prd+", null ,null, null, null")
	}

	if err := rows.Err(); err != nil {
		return nil, false, err
	}

	if len(black_list_data) == 0 {
		return []string{}, true, err
	}

	return black_list_data, true, nil
}

func Get_blacklist_OLD(c *gin.Context) {
	var reqBody GetBlackList

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

	}

	db, _ := db_client.DbConnection()
	defer db.Close()

	if data, isSuccess, _ := Get_blacklist_db(db, reqBody.From, reqBody.To); isSuccess {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"data":    data,
			"message": "検索に成功しました。",
		})
	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"data":    nil,
			"message": "システムの不明なエラーです。",
		})

	}
}

func Get_blacklist(c *gin.Context) {
	token := strings.Split(c.Request.Header["Authorization"][0], " ")[1]

	if token != utils.ACCESS_TOKEN {
		c.JSON(http.StatusOK, gin.H{
			"code":           "401",
			"token_request":  token,
			"token_validate": utils.ACCESS_TOKEN,
			"message":        "Sai thông tin chứng thực",
		})
		return
	}

	db, _ := db_client.DbConnection()
	defer db.Close()

	if data, isSuccess, _ := Get_blacklist_db_NEW(db); isSuccess {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"content": data,
			"message": "検索に成功しました。",
		})
	} else {
		c.JSON(http.StatusNotFound, gin.H{
			"code":    "404",
			"content": nil,
			"message": "システムの不明なエラーです。",
		})

	}
}
