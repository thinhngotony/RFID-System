package controllers

import (
	"main/db_client"
	"main/utils"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/go-playground/validator"
)

type InsertLog struct {
	ApiKey string `json:"api_key" validate:"required"`
	RFID   string `json:"rfid" validate:"required"`
	Mode   string `json:"mode" validate:"required"`
}

func InsertDataToLogTable(c *gin.Context) {
	var reqBody InsertLog
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

	if data_exist, _, _ := db_client.CheckExistRFID_Master(db, []string{reqBody.RFID}); len(data_exist) != 0 {

		if success, create_date, shop_code, err := db_client.InsertSingleRowToLogTable(db, reqBody.RFID, reqBody.Mode); err != nil {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.UNKNOWN_ERROR_CODE,
				"data":    nil,
				"message": "システムの不明なエラーです。",
			})
		} else if success {
			data := gin.H{
				"create_date": create_date,
				"shop_code":   shop_code,
				"rfid":        reqBody.RFID,
				"mode":        reqBody.Mode,
			}
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.SUCCESSFULLY_CODE,
				"data":    data,
				"message": "登録に成功しました。",
			})
		}

	} else if len(data_exist) == 0 {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.DATA_NOT_EXIST_CODE,
			"data":    nil,
			"message": "RFIDは存在しません。",
		})
	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"data":    nil,
			"message": "システムの不明なエラーです。",
		})
	}
}
