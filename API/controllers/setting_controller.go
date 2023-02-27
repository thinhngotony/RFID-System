package controllers

import (
	"main/db_client"
	"main/utils"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/go-playground/validator"
)

type Set_Gate_Setting struct {
	ApiKey              string `json:"api_key" validate:"required"`
	Use_gate_checkpoint int    `json:"use_gate_checkpoint"`
	Url_gate_checkpoint string `json:"url_gate_checkpoint"`
	Username_gate       string `json:"username_gate"`
	Password_gate       string `json:"password_gate"`
}

type Get_Gate_Setting struct {
	ApiKey string `json:"api_key" validate:"required"`
}

func GetGateSetting(c *gin.Context) {
	var reqBody Get_Gate_Setting

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

	if data, isSuccess, _ := db_client.GetGateSetting(db); isSuccess {
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

func SetGateSetting(c *gin.Context) {
	var reqBody Set_Gate_Setting

	if err := c.ShouldBindJSON(&reqBody); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.INVALID_FORMAT_CODE,
			"message": "jsonリクエストの本文の形式が正しくありません。",
		})
		return

	} else if err := validator.New().Struct(reqBody); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.MISSING_DATA_CODE,
			"message": "リクエストパラメータが不足しています。",
		})
		return

	} else if err := utils.VerifyApiKey(reqBody.ApiKey); !err {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.INVALID_API_KEY_CODE,
			"message": "APIキーが正しくありません。",
		})
		return
	}

	db, _ := db_client.DbConnection()
	defer db.Close()

	if isSuccess, _ := db_client.SetGateSetting(db, reqBody.Use_gate_checkpoint, reqBody.Url_gate_checkpoint, reqBody.Username_gate, reqBody.Password_gate); isSuccess {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"message": "登録に成功しました。",
		})
	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"message": "システムの不明なエラーです。",
		})

	}

}
