package controllers

import (
	"main/db_client"
	"main/utils"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/go-playground/validator"
)

func EncodeImage(c *gin.Context) {
	var reqBody ReqBody_Image_Set

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

	if err := db_client.Base64toJpg(reqBody.Base64, reqBody.ISBN); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"data":    nil,
			"message": "システムの不明なエラーです。",
		})
	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"error":   nil,
			"message": "変換に成功しました。",
		})
	}

}

func DecodeImage(c *gin.Context) {
	var reqBody ReqBody_Image_Get

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

	if data, err := db_client.GetJPEGbase64(reqBody.ISBN); data == "DATA_NOT_EXIST_CODE" {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.DATA_NOT_EXIST_CODE,
			"data":    nil,
			"error":   err,
			"message": "画像が見つかりません。",
		})
	} else if err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"data":    nil,
			"error":   err,
			"message": "システムの不明なエラーです。",
		})

	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"data":    data,
			"error":   nil,
			"message": "変換に成功しました。",
		})
	}

}

func Test(c *gin.Context) {
	var reqBody ReqBody_Image_Get

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

	var cover string
	//var base64 string

	//base64, _ = db_client.GetJPEGbase64(reqBody.ISBN)
	cover, _ = db_client.CallAPIGetImageJapan(reqBody.ISBN)

	if base64, err := db_client.GetJPEGbase64(reqBody.ISBN); base64 == "DATA_NOT_EXIST_CODE" {
		if cover, err = db_client.CallAPIGetImageJapan(reqBody.ISBN); err != nil {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.UNKNOWN_ERROR_CODE,
				"base64":  nil,
				"cover":   nil,
				"error":   err,
				"message": "システムの不明なエラーです。",
			})
		} else if cover == "" {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.DATA_NOT_EXIST_CODE,
				"base64":  nil,
				"cover":   nil,
				"error":   err,
				"message": "画像が見つかりません。",
			})
		} else {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.SUCCESSFULLY_CODE,
				"base64":  nil,
				"cover":   cover,
				"error":   nil,
				"message": "変換に成功しました。",
			})
		}
	} else if err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"base64":  nil,
			"cover":   nil,
			"error":   err,
			"message": "システムの不明なエラーです。",
		})

	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"base64":  base64,
			"cover":   cover,
			"error":   nil,
			"message": "変換に成功しました。",
		})
	}

}
