package Common

import (
	"encoding/json"
	"github.com/goframework/gf/exterror"
)

type KeisanParameter struct {
	AnalyzeDateFrom            int64
	AnalyzeDateTo              int64
	SimulationWeightFrom       float64
	SimulationWeightTo         float64
	SimulationWeightStep       float64
	SimulationSaleCollectDays  int64
	SimulationSaleForecastDays int64
	SimulationOrderRemainDays  int64
	MaxStockCount              int64
	MinStockCountFrom          int64
	MinStockCountTo            int64
	MaxOrderCount              int64
	MinOrderCount              int64
	ExcessDays                 int64
	LackPercent                int64
}

func UnmarshalKeisanParameter(s string) (*KeisanParameter, *ErrorDetail) {
	kp := KeisanParameter{}
	err := json.Unmarshal([]byte(s), &kp)
	if err != nil {
		return nil, &ErrorDetail{exterror.WrapExtError(err), "Error on unmarshal keisan parameter"}
	}

	return &kp, nil
}

type GoodsSearchCondition struct {
	FranchiseCd   []string
	ShopCd        []string
	BaseFranchise []string
	BaseShop      []string
	MediaCd1      []string
	MediaCd2      []string
	MediaCd3      []string
	MediaCd4      []string
	SaleDateFrom  int64
	SaleDateTo    int64
	PriceFrom     int64
	PriceTo       int64
	Writer        string
	Publisher     string
	RankType      string
	RankLvl       int64
}

func UnmarshalGoodsSearchCondition(s string) (*GoodsSearchCondition, *ErrorDetail) {
	gsc := GoodsSearchCondition{}
	err := json.Unmarshal([]byte(s), &gsc)
	if err != nil {
		return nil, &ErrorDetail{exterror.WrapExtError(err), "Error on unmarshal GoodsSearchCondition"}
	}

	return &gsc, nil
}
