/*
Google cloud platform service providers
*/
package gcp

import (
	"context"
	"io/ioutil"
	"main/gcp/bq"
	"main/gcp/st"
	"os"
	"path/filepath"

	"github.com/goframework/gf/ext"
	"golang.org/x/oauth2/google"
	"golang.org/x/oauth2/jwt"
	"google.golang.org/api/bigquery/v2"
	"google.golang.org/api/storage/v1"
)

type GCPConfig struct {
	PrivateKeyPem string
	Email         string
	ProjectID     string
}

func NewBQUtil(cfg *GCPConfig) (*bq.Util, error) {
	pKeyFilePath := cfg.PrivateKeyPem
	if !ext.FileExists(pKeyFilePath) && !filepath.IsAbs(pKeyFilePath) {
		pKeyFilePath = filepath.Join(filepath.Dir(os.Args[0]), pKeyFilePath)
	}
	pemKeyBytes, err := ioutil.ReadFile(pKeyFilePath)
	if err != nil {
		return nil, err
	}

	// Credentials obtained from the Google
	conf := &jwt.Config{
		Email:      cfg.Email,
		PrivateKey: pemKeyBytes,
		Scopes: []string{
			bigquery.BigqueryScope,
			bigquery.BigqueryInsertdataScope,
		},
		TokenURL: google.JWTTokenURL,
	}

	// Initiate an http.Client
	client := conf.Client(context.Background())
	bqService, err := bigquery.New(client)

	if err != nil {
		return nil, err
	}

	// Verify service by get datasets list
	_, err = bqService.Datasets.List(cfg.ProjectID).Do()
	if err != nil {
		return nil, err
	}

	mBQUtil := &bq.Util{Service: bqService, ProjectId: cfg.ProjectID}

	return mBQUtil, nil
}

func NewGSUtil(cfg *GCPConfig) (*st.Util, error) {
	pKeyFilePath := cfg.PrivateKeyPem
	if !ext.FileExists(pKeyFilePath) && !filepath.IsAbs(pKeyFilePath) {
		pKeyFilePath = filepath.Join(filepath.Dir(os.Args[0]), pKeyFilePath)
	}
	pemKeyBytes, err := ioutil.ReadFile(pKeyFilePath)

	if err != nil {
		return nil, err
	}

	// Credentials obtained from the Google
	conf := &jwt.Config{
		Email:      cfg.Email,
		PrivateKey: pemKeyBytes,
		Scopes: []string{
			storage.CloudPlatformScope,
			storage.DevstorageReadWriteScope,
		},
		TokenURL: google.JWTTokenURL,
	}

	// Initiate an http.Client
	client := conf.Client(context.Background())
	storageService, err := storage.New(client)

	if err != nil {
		return nil, err
	}

	// Verify service by get buckets list
	_, err = storageService.Buckets.List(cfg.ProjectID).Do()
	if err != nil {
		return nil, err
	}

	mSTUtil := &st.Util{
		Service:   storageService,
		ProjectId: cfg.ProjectID,
	}

	return mSTUtil, nil
}
