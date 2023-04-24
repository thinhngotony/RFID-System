package controllers

import (
	"main/db_client"
	"main/utils"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/go-playground/validator"
)

type JAN_to_RFID struct {
	ApiKey  string `json:"api_key" validate:"required"`
	JanCode string `json:"jan_code" validate:"required"`
	JanType int    `json:"jan_type" validate:"required,gte=1,lte=2"`
}

func GetRFIDfromJanCode(c *gin.Context) {
	var reqBody JAN_to_RFID
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

	if reqBody.JanType == 1 {
		if rfid_list, data_exist, err := db_client.ConvertFromJan1(db, reqBody.JanCode); err != nil {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.UNKNOWN_ERROR_CODE,
				"data":    nil,
				"message": "システムの不明なエラーです。",
			})
		} else {
			data := gin.H{
				"rfid_list": rfid_list,
				"jancode":   reqBody.JanCode,
			}
			if data_exist {
				c.JSON(http.StatusOK, gin.H{
					"code":    utils.SUCCESSFULLY_CODE,
					"data":    data,
					"message": "変換に成功しました。",
				})
			} else {
				c.JSON(http.StatusOK, gin.H{
					"code":    utils.DATA_NOT_EXIST_CODE,
					"data":    nil,
					"message": "JANコードは存在しません。",
				})
			}
		}

	} else if reqBody.JanType == 2 {
		if rfid_list, data_exist, err := db_client.ConvertFromJan2(db, reqBody.JanCode); err != nil {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.UNKNOWN_ERROR_CODE,
				"data":    nil,
				"message": "システムの不明なエラーです。",
			})
		} else {
			data := gin.H{
				"rfid_list": rfid_list,
				"jancode":   reqBody.JanCode,
			}
			if data_exist {
				c.JSON(http.StatusOK, gin.H{
					"code":    utils.SUCCESSFULLY_CODE,
					"data":    data,
					"message": "変換に成功しました。",
				})
			} else {
				c.JSON(http.StatusOK, gin.H{
					"code":    utils.DATA_NOT_EXIST_CODE,
					"data":    nil,
					"message": "JANコードは存在しません。",
				})
			}
		}
	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"data":    nil,
			"message": "システムの不明なエラーです。",
		})
	}
}
