package controllers

import (
	"main/db_client"
	"main/utils"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/go-playground/validator"
)

type Get_SmartSelf_Names struct {
	ApiKey string `json:"api_key" validate:"required"`
}

type Get_SmartSelf_Setting struct {
	ApiKey         string `json:"api_key" validate:"required"`
	Dpp_shelf_name string `json:"dpp_shelf_name" validate:"required"`
}

type Get_SmartSelf_Log_Setting struct {
	ApiKey string `json:"api_key" validate:"required"`
	Rfid   string `json:"rfid" validate:"required"`
}

func GetSmartSelfSetting(c *gin.Context) {
	var reqBody Get_SmartSelf_Setting

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

	db, _ := db_client.DbConnection_smartself()
	defer db.Close()

	if data, isSuccess, _ := db_client.GetSmartSelfSetting(db, reqBody.Dpp_shelf_name); isSuccess {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"data":    data,
			"message": "検索に成功しました。",
		})
	} else {
		if isExist := db_client.ShelfExists(db, reqBody.Dpp_shelf_name); !isExist {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.DATA_NOT_EXIST_CODE,
				"data":    nil,
				"message": "データが存在しません。",
			})
		} else {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.UNKNOWN_ERROR_CODE,
				"data":    nil,
				"message": "システムの不明なエラーです。",
			})
		}

	}
}

func GetShelfNames(c *gin.Context) {
	var reqBody Get_SmartSelf_Names

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

	db, _ := db_client.DbConnection_smartself()
	defer db.Close()

	if data, err := db_client.GetShelfNames(db); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"message": "システムの不明なエラーです。",
		})
	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"data":    data,
			"message": "検索に成功しました。",
		})

	}
}

func SetSmartSelfSetting(c *gin.Context) {
	var reqBody db_client.Set_SmartSelf_Setting

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

	db, _ := db_client.DbConnection_smartself()
	defer db.Close()

	if isDuplicate, _ := db_client.CheckExistRFID_SmartSelf(db, reqBody.Dpp_rfid_cd, true); !isDuplicate {
		if isExist, _ := db_client.CheckExistPosition(db, reqBody); !isExist {
			if isSuccess, _ := db_client.InsertSmartSelfSetting(db, reqBody); isSuccess {
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

		} else if isExist {
			if isSuccess, _ := db_client.UpdateSmartSelfSetting(db, reqBody); isSuccess {
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
	}
}

func GetSmartSelfLogSetting(c *gin.Context) {
	var reqBody Get_SmartSelf_Log_Setting

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

	db, _ := db_client.DbConnection_smartself()
	defer db.Close()

	if data, isExist, _ := db_client.CheckStatusRFID_SmartSelf(db, reqBody.Rfid); isExist {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"data":    data,
			"message": "検索に成功しました。",
		})
	} else if !isExist {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.DATA_NOT_EXIST_CODE,
			"data":    data,
			"message": "不正なRFIDコードが存在しています。",
		})

	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"data":    nil,
			"message": "システムの不明なエラーです。",
		})
	}
}
