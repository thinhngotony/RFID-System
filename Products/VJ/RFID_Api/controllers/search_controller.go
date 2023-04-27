package controllers

import (
	"fmt"
	"main/db_client"
	"main/utils"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/go-playground/validator"
)

type Search_from_Jan struct {
	ApiKey    string `json:"api_key" validate:"required"`
	JanCode   string `json:"jancode"`
	GoodsName string `json:"goods_name"`
}

func SearchfromJan(c *gin.Context) {
	var reqBody Search_from_Jan

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

	if reqBody.JanCode != "" {
		if data, data_exist, _ := db_client.SearchFromJan(db, reqBody.JanCode); data_exist {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.SUCCESSFULLY_CODE,
				"data":    data,
				"message": "検索に成功しました。",
			})
		} else if !data_exist {
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
	} else if reqBody.JanCode == "" {
		if data, data_exist, _ := db_client.SearchFromName(db, reqBody.GoodsName); data_exist {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.SUCCESSFULLY_CODE,
				"data":    data,
				"message": "検索に成功しました。",
			})
		} else if !data_exist {
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
	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"data":    nil,
			"message": "システムの不明なエラーです。",
		})
	}
}

func SearchFromJan_BQ(c *gin.Context) {
	var reqBody Search_from_Jan

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
	fmt.Println(reqBody.JanCode)

	if reqBody.JanCode != "" {
		if data, data_exist, _ := db_client.SearchFromJan_BQ(db, reqBody.JanCode); data_exist {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.SUCCESSFULLY_CODE,
				"data":    data,
				"message": "検索に成功しました。",
			})
		} else if !data_exist {
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
	} else if reqBody.JanCode == "" {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.MISSING_DATA_CODE,
			"data":    nil,
			"message": "リクエストパラメータが不足しています。",
		})
	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"data":    nil,
			"message": "システムの不明なエラーです。",
		})
	}

}
