package controllers

import (
	"main/db_client"
	"main/utils"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/go-playground/validator"
)

type ReqBody struct {
	ApiKey string `json:"api_key" validate:"required"`
}

type ReqBody_Paging struct {
	ApiKey string `json:"api_key" validate:"required"`
	Page   int64  `json:"page" validate:"required"`
	Rows   int64  `json:"rows" validate:"required"`

	Drsv_shop_cd     string `json:"drsv_shop_cd"`
	Drsv_pos_no_from string `json:"drsv_pos_no_from"`
	Drsv_pos_no_to   string `json:"drsv_pos_no_to"`

	Drsv_receipt_no_from string `json:"drsv_receipt_no_from"`
	Drsv_receipt_no_to   string `json:"drsv_receipt_no_to"`

	Drsv_date_from string `json:"drsv_date_from"`
	Drsv_date_to   string `json:"drsv_date_to"`

	Drsv_start_time string `json:"drsv_start_time"`
	Drsv_end_time   string `json:"drsv_end_time"`

	Drsv_customer_base_from string `json:"drsv_customer_base_from"`
	Drsv_customer_base_to   string `json:"drsv_customer_base_to"`
}

type ReqBody_Image_Set struct {
	ApiKey string `json:"api_key" validate:"required"`
	Base64 string `json:"base64" validate:"required"`
	ISBN   string `json:"isbn" validate:"required"`
}

type ReqBody_Image_Get struct {
	ApiKey string `json:"api_key" validate:"required"`
	ISBN   string `json:"isbn" validate:"required"`
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
	var reqBody ReqBody

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

func SetSmartSelfLocation(c *gin.Context) {
	var reqBody ReqBody

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

	if isSuccess, _ := db_client.ClearSmartSelfLocation(db); isSuccess {
		if isSuccess, _ := db_client.ConvertToDecimalSmartSelfLocation(db); isSuccess {
			if isSuccess, _ := db_client.SetAverageValueForRSSI(db); isSuccess {
				if isSuccess, _ := db_client.SetSmartSelfLocation(db); isSuccess {
					if isSuccess, _ := db_client.DeleteInvalidRecord(db); isSuccess {
						c.JSON(http.StatusOK, gin.H{
							"code":    utils.SUCCESSFULLY_CODE,
							"message": "登録に成功しました。",
						})
					}

				}
			}
		}

	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"message": "システムの不明なエラーです。",
		})
	}
}

func SetSmartShelfLocation(c *gin.Context) {
	var reqBody ReqBody

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

	if isSuccess, _ := db_client.ClearSmartSelfLocation(db); isSuccess {
		if isSuccess, _ := db_client.ConvertToDecimalSmartSelfLocation(db); isSuccess {
			if isSuccess, _ := db_client.SetSmartSelfLocation(db); isSuccess {
				c.JSON(http.StatusOK, gin.H{
					"code":    utils.SUCCESSFULLY_CODE,
					"message": "登録に成功しました。",
				})
			}
		}

	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"message": "システムの不明なエラーです。",
		})
	}
}
func ClearRawData(c *gin.Context) {
	var reqBody ReqBody

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

	if isSuccess, _ := db_client.ClearSmartSelfLocationRAW(db); isSuccess {
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

func SetSmartSelfMoreInfo(c *gin.Context) {
	var reqBody db_client.Get_SmartSelf_Location_2

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

	if isSuccess, _ := db_client.SetSmartSelfMoreInfo(db, reqBody.EPC, reqBody.Jancode, reqBody.Product_name, reqBody.Link_image); isSuccess {
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

func GetSmartSelfLocation(c *gin.Context) {
	var reqBody db_client.Get_SmartSelf_Location

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

	if data, isSuccess, _ := db_client.GetSmartSelfLocation(db, reqBody.Shelf_no); isSuccess {
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

func GetSmartSelfLocationByCol(c *gin.Context) {
	var reqBody db_client.Get_SmartSelf_Location

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

	if data, isSuccess, _ := db_client.GetSmartSelfLocationByCol(db, reqBody.Shelf_no, reqBody.Col, reqBody.Row); isSuccess {
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

func ClearPositionMSTAntena(c *gin.Context) {
	var reqBody db_client.Clear_SmartShelf_Position_mst_antena

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

	if isSuccess, err := db_client.ClearDirectionMSTAntena(db, reqBody.Shelf_no); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"message": err,
		})
	} else if isSuccess {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"message": "登録に成功しました。",
		})

	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"message": err,
		})
	}

}

func UpdatePositionMSTAntena_OLD(c *gin.Context) {
	var reqBody db_client.Set_SmartShelf_Position_mst_antena

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

	if isExist, err := db_client.CheckExistKeyInMST(db, reqBody.Shelf_no, reqBody.Antena_no); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"message": err,
		})
	} else {
		if isExist {
			//Update only
			if isSuccess, err := db_client.UpdatePositionMSTAntena(db, reqBody); isSuccess {
				c.JSON(http.StatusOK, gin.H{
					"code":    utils.SUCCESSFULLY_CODE,
					"message": "登録に成功しました。",
				})
			} else {
				c.JSON(http.StatusOK, gin.H{
					"code":    utils.UNKNOWN_ERROR_CODE,
					"message": err,
				})
			}
		} else {
			//Insert with condition not exist data
			if isSuccess, err := db_client.InsertPositionMSTAntena(db, reqBody); isSuccess {
				c.JSON(http.StatusOK, gin.H{
					"code":    utils.SUCCESSFULLY_CODE,
					"message": "登録に成功しました。",
				})
			} else {
				c.JSON(http.StatusOK, gin.H{
					"code":    utils.UNKNOWN_ERROR_CODE,
					"message": err,
				})
			}
		}

	}
}

func UpdatePositionMSTAntena(c *gin.Context) {
	var reqBody db_client.Set_SmartShelf_Position_mst_antena

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

	//Insert with condition not exist data
	if isSuccess, err := db_client.InsertPositionMSTAntena(db, reqBody); isSuccess {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"message": "登録に成功しました。",
		})
	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"message": err,
		})
	}

}

func UpdatePositionMSTAntenaV1(c *gin.Context) {
	var reqBody db_client.Set_SmartShelf_Position_mst_antena

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

	if isExist, err := db_client.CheckExistKeyInMST(db, reqBody.Shelf_no, reqBody.Antena_no); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"message": err,
		})
	} else {
		if isExist {
			//Update only
			if isSuccess, err := db_client.UpdatePositionMSTAntena(db, reqBody); isSuccess {
				c.JSON(http.StatusOK, gin.H{
					"code":    utils.SUCCESSFULLY_CODE,
					"message": "登録に成功しました。",
				})
			} else {
				c.JSON(http.StatusOK, gin.H{
					"code":    utils.UNKNOWN_ERROR_CODE,
					"message": err,
				})
			}
		} else {
			//Insert with condition not exist data
			if isSuccess, err := db_client.InsertPositionMSTAntena(db, reqBody); isSuccess {
				c.JSON(http.StatusOK, gin.H{
					"code":    utils.SUCCESSFULLY_CODE,
					"message": "登録に成功しました。",
				})
			} else {
				c.JSON(http.StatusOK, gin.H{
					"code":    utils.UNKNOWN_ERROR_CODE,
					"message": err,
				})
			}
		}

	}
}

func LoadPositionMSTAntena(c *gin.Context) {
	var reqBody db_client.Get_SmartShelf_Position_mst_antena

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

	// if _, err := db_client.CheckExistKeyInMST(db, reqBody.Shelf_no, reqBody.Antena_no); err != nil {
	// 	c.JSON(http.StatusOK, gin.H{
	// 		"code":    utils.UNKNOWN_ERROR_CODE,
	// 		"message": err,
	// 	})
	// } else {

	if data, err := db_client.LoadPositionMSTAntena(db, *reqBody.Shelf_no); err != nil {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.UNKNOWN_ERROR_CODE,
			"data":    nil,
			"message": err,
		})
	} else {
		c.JSON(http.StatusOK, gin.H{
			"code":    utils.SUCCESSFULLY_CODE,
			"data":    data,
			"message": "登録に成功しました。",
		})
	}
}
