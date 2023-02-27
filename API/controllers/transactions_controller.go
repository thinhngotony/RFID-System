package controllers

import (
	"main/db_client"
	"main/utils"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/go-playground/validator"
)

func InsertDataToTransactionTable(c *gin.Context) {
	var reqBody db_client.InsertTransaction

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

	if isSuccess, err := db_client.InsertTransaction_func(db,
		reqBody.Drtr_receipt_no,
		reqBody.Drtr_shop_no,
		reqBody.Drtr_pos_no,
		reqBody.Drtr_date_create,
		reqBody.Drtr_start_time,
		reqBody.Drtr_end_time,
		reqBody.Drtr_is_saved); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"message": err,
		})

	} else if isSuccess {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"message": "登録に成功しました。",
		})
	}
}

func Set_is_saved_transaction(c *gin.Context) {
	var reqBody db_client.InsertTransaction

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

	if isSuccess, err := db_client.UpdateTransactions(db,
		reqBody.Drtr_is_saved,
		reqBody.Drtr_receipt_no); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"message": err,
		})

	} else if isSuccess {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"message": err,
		})
	}
}

func Get_not_saved_transaction(c *gin.Context) {
	var reqBody db_client.InsertTransaction

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

	if data, isSuccess, err := db_client.Get_not_saved_transaction_func(db); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"message": err,
		})

	} else if isSuccess {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"data":    data,
			"message": err,
		})
	}
}
