package controllers

import (
	"main/db_client"
	"main/utils"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/go-playground/validator"
)

type RFIDList struct {
	ApiKey   string   `json:"api_key" validate:"required"`
	RFIDList []string `json:"rfid" validate:"required"`
}

func ClassifyRFID(rfid_sold []db_client.RFID_Status, rfid_unsold []db_client.RFID_Status) gin.H {
	valid_rfid := gin.H{
		"rfid_sold":   rfid_sold,
		"rfid_unsold": rfid_unsold,
	}
	return valid_rfid
}

func GetJanCodefromRFIDList(c *gin.Context) {
	var reqBody RFIDList

	if err := c.ShouldBindJSON(&reqBody); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.INVALID_FORMAT_CODE,
			"data":    nil,
			"error":   nil,
			"message": "jsonリクエストの本文の形式が正しくありません。",
		})
		return

	} else if err := validator.New().Struct(reqBody); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.MISSING_DATA_CODE,
			"data":    nil,
			"error":   nil,
			"message": "リクエストパラメータが不足しています。",
		})
		return

	} else if err := utils.VerifyApiKey(reqBody.ApiKey); !err {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.INVALID_API_KEY_CODE,
			"data":    nil,
			"error":   nil,
			"message": "APIキーが正しくありません。",
		})
		return
	}

	db, _ := db_client.DbConnection()
	defer db.Close()

	if valid_rfid, invalid_rfid := db_client.GetDataFromRFIDList(db, reqBody.RFIDList); valid_rfid != nil && invalid_rfid == nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"data":    valid_rfid,
			"error":   nil,
			"message": "変換に成功しました。",
		})
	} else if invalid_rfid != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.DATA_NOT_EXIST_CODE,
			"data":    valid_rfid,
			"error":   invalid_rfid,
			"message": "不正なRFIDコードが存在しています。",
		})
	} else if len(reqBody.RFIDList) == 0 {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.MISSING_DATA_CODE,
			"data":    nil,
			"error":   nil,
			"message": "リクエストパラメータが不足しています。",
		})
	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"data":    nil,
			"error":   nil,
			"message": "システムの不明なエラーです。",
		})
	}

}

func GetStatusfromRFIDList(c *gin.Context) {
	var reqBody RFIDList

	if err := c.ShouldBindJSON(&reqBody); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.INVALID_FORMAT_CODE,
			"data":    nil,
			"error":   nil,
			"message": "JSONリクエストの本文の形式が正しくありません。",
		})
		return

	} else if err := validator.New().Struct(reqBody); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.MISSING_DATA_CODE,
			"data":    nil,
			"error":   nil,
			"message": "リクエストパラメータが不足しています。",
		})
		return

	} else if err := utils.VerifyApiKey(reqBody.ApiKey); !err {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.INVALID_API_KEY_CODE,
			"data":    nil,
			"error":   nil,
			"message": "APIキーが正しくありません。",
		})
		return
	}

	db, _ := db_client.DbConnection()
	defer db.Close()

	if rfid_sold, rfid_unsold, invalid_rfid := db_client.GetStatusFromRFIDList(db, reqBody.RFIDList); rfid_sold != nil && invalid_rfid == nil {
		valid_rfid := ClassifyRFID(rfid_sold, rfid_unsold)
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"data":    valid_rfid,
			"error":   nil,
			"message": "変換に成功しました。",
		})
	} else if rfid_unsold != nil && invalid_rfid == nil {
		valid_rfid := ClassifyRFID(rfid_sold, rfid_unsold)
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"data":    valid_rfid,
			"error":   invalid_rfid,
			"message": "変換に成功しました。",
		})
	} else if invalid_rfid != nil {
		valid_rfid := ClassifyRFID(rfid_sold, rfid_unsold)
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.DATA_NOT_EXIST_CODE,
			"data":    valid_rfid,
			"error":   invalid_rfid,
			"message": "不正なRFIDコードが存在しています。",
		})

	} else if len(reqBody.RFIDList) == 0 {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.MISSING_DATA_CODE,
			"data":    nil,
			"error":   nil,
			"message": "リクエストパラメータが不足しています。",
		})
	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"data":    nil,
			"error":   nil,
			"message": "システムの不明なエラーです。",
		})
	}

}
