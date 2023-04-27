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
			"message": "Invalid Format",
		})
		return

	} else if err := validator.New().Struct(reqBody); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.MISSING_DATA_CODE,
			"data":    nil,
			"message": "Missing Data",
		})
		return

	} else if err := utils.VerifyApiKey(reqBody.ApiKey); !err {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.INVALID_API_KEY_CODE,
			"data":    nil,
			"message": "Invalid API Key",
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
				"message": "Success",
			})
		} else if !data_exist {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.DATA_NOT_EXIST_CODE,
				"data":    nil,
				"message": "Data not exist",
			})
		} else {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.UNKNOWN_ERROR_CODE,
				"data":    nil,
				"message": "Unknown Error",
			})

		}
	} else if reqBody.JanCode == "" {
		if data, data_exist, _ := db_client.SearchFromName(db, reqBody.GoodsName); data_exist {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.SUCCESSFULLY_CODE,
				"data":    data,
				"message": "Success",
			})
		} else if !data_exist {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.DATA_NOT_EXIST_CODE,
				"data":    nil,
				"message": "Data not exist",
			})
		} else {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.UNKNOWN_ERROR_CODE,
				"data":    nil,
				"message": "Unknown Error",
			})

		}
	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"data":    nil,
			"message": "Unknown Error",
		})
	}
}

func SearchFromJan_BQ(c *gin.Context) {
	var reqBody Search_from_Jan

	if err := c.ShouldBindJSON(&reqBody); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.INVALID_FORMAT_CODE,
			"data":    nil,
			"message": "Invalid Format",
		})
		return

	} else if err := validator.New().Struct(reqBody); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.MISSING_DATA_CODE,
			"data":    nil,
			"message": "Missing Data",
		})
		return

	} else if err := utils.VerifyApiKey(reqBody.ApiKey); !err {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.INVALID_API_KEY_CODE,
			"data":    nil,
			"message": "Invalid API Key",
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
				"message": "Success",
			})
		} else if !data_exist {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.DATA_NOT_EXIST_CODE,
				"data":    nil,
				"message": "Data not exist",
			})
		} else {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.UNKNOWN_ERROR_CODE,
				"data":    nil,
				"message": "Unknown Error",
			})

		}
	} else if reqBody.JanCode == "" {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.MISSING_DATA_CODE,
			"data":    nil,
			"message": "Missing Data",
		})
	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"data":    nil,
			"message": "Unknown Error",
		})
	}

}
