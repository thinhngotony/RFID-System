package controllers

import (
	"main/Common"
	"main/utils"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/go-playground/validator"
)

type Drfid_selfregi_video struct {
	ApiKey             string `json:"api_key"`
	Drsv_shop_cd       string `json:"drsv_shop_cd" sql:"drsv_shop_cd"`
	Drsv_pos_no        string `json:"drsv_pos_no" sql:"drsv_pos_no"`
	Drsv_receipt_no    string `json:"drsv_receipt_no" sql:"drsv_receipt_no"`
	Drsv_date          string `json:"drsv_date" sql:"drsv_date"`
	Drsv_start_time    string `json:"drsv_start_time" sql:"drsv_start_time"`
	Drsv_end_time      string `json:"drsv_end_time" sql:"drsv_end_time"`
	Drsv_customer_base string `json:"drsv_customer_base" sql:"drsv_customer_base"`
	Drsv_video_link    string `json:"drsv_video_link" sql:"drsv_video_link"`
	Drsv_thumnail      string `json:"drsv_thumnail" sql:"drsv_thumnail"`
}

func InsertSelfRegiBQ(c *gin.Context) {
	var reqBody Common.Drfid_selfregi_video

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

	if err := Common.InsertSelfRegiBQ(reqBody); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"data":    nil,
			"error":   err,
			"message": "システムの不明なエラーです。",
		})

	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"data":    reqBody,
			"error":   nil,
			"message": "登録に成功しました。",
		})
	}

}

func GetDataSelfRegiBQ(c *gin.Context) {
	var reqBody ReqBody_Paging

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

	if data, err := Common.GetDataSelfRegiBQ(reqBody.Page, reqBody.Rows); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"data":    nil,
			"error":   err,
			"message": "システムの不明なエラーです。",
		})

	} else {
		if total_rows, err := Common.GetTotalRowsSelfRegiBQ(); err != nil {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.UNKNOWN_ERROR_CODE,
				"data":    nil,
				"error":   err,
				"message": "システムの不明なエラーです。",
			})
		} else {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.SUCCESSFULLY_CODE,
				"rows":    total_rows,
				"data":    data,
				"error":   nil,
				"message": "登録に成功しました。",
			})
		}
	}

}

func GetDataSelfRegiBQ_NEW(c *gin.Context) {
	var reqBody ReqBody_Paging

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

	//reflect.TypeOf(ReqBody_Paging{}).NumField()

	// for _, v := range ReqBody_Paging {
	// 	reflect.ValueOf(v)
	// }

	if data, err := Common.GetDataSelfRegiBQ_NEW(reqBody.Page,
		reqBody.Rows,
		reqBody.Drsv_shop_cd,
		reqBody.Drsv_pos_no_from,
		reqBody.Drsv_pos_no_to,
		reqBody.Drsv_receipt_no_from,
		reqBody.Drsv_receipt_no_to,
		reqBody.Drsv_date_from,
		reqBody.Drsv_date_to,
		reqBody.Drsv_start_time,
		reqBody.Drsv_end_time,
		reqBody.Drsv_customer_base_from,
		reqBody.Drsv_customer_base_to); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"data":    nil,
			"error":   err,
			"message": "システムの不明なエラーです。",
		})

	} else {
		if total_rows, err := Common.GetTotalRowsSelfRegiBQ_New(reqBody.Drsv_shop_cd,
			reqBody.Drsv_pos_no_from,
			reqBody.Drsv_pos_no_to,
			reqBody.Drsv_receipt_no_from,
			reqBody.Drsv_receipt_no_to,
			reqBody.Drsv_date_from,
			reqBody.Drsv_date_to,
			reqBody.Drsv_start_time,
			reqBody.Drsv_end_time,
			reqBody.Drsv_customer_base_from,
			reqBody.Drsv_customer_base_to); err != nil {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.UNKNOWN_ERROR_CODE,
				"data":    nil,
				"error":   err,
				"message": "システムの不明なエラーです。",
			})
		} else {
			c.JSON(http.StatusOK, gin.H{
				"code":    utils.SUCCESSFULLY_CODE,
				"rows":    total_rows,
				"data":    data,
				"error":   nil,
				"message": "登録に成功しました。",
			})
		}
	}
}
