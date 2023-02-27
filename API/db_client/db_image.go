package db_client

import (
	"bufio"
	"encoding/base64"
	"encoding/json"
	"fmt"
	"image"
	"image/jpeg"
	"image/png"
	"io/ioutil"
	"main/utils"
	"net/http"
	"strings"

	"log"
	"os"

	"github.com/goframework/gf/exterror"
)

type OpenDbSummary struct {
	Isbn      string `json:"isbn"`
	Title     string `json:"title"`
	Volume    string `json:"volume"`
	Series    string `json:"series"`
	Publisher string `json:"publisher"`
	Pubdate   string `json:"pubdate"`
	Cover     string `json:"cover"`
	Author    string `json:"author"`
}

type OpenDbResponse struct {
	Onix    interface{}   `json:"onix"`
	Hanmoto interface{}   `json:"hanmoto"`
	Summary OpenDbSummary `json:"summary"`
}

//Take an existing jpg srcFileName and decode/encode it
func CreateJpg() {

	srcFileName := "flower.jpg"
	dstFileName := "newFlower.jpg"
	// Decode the JPEG data. If reading from file, create a reader with
	reader, err := os.Open(srcFileName)
	if err != nil {
		log.Fatal(err)
	}
	defer reader.Close()
	//Decode from reader to image format
	m, formatString, err := image.Decode(reader)
	if err != nil {
		log.Fatal(err)
		return
	}
	fmt.Println("Got format String", formatString)
	fmt.Println(m.Bounds())

	//Encode from image format to writer
	f, err := os.OpenFile(dstFileName, os.O_WRONLY|os.O_CREATE, 0777)
	if err != nil {
		log.Fatal(err)
		return
	}

	err = jpeg.Encode(f, m, &jpeg.Options{Quality: 75})
	if err != nil {
		log.Fatal(err)
		return
	}
	fmt.Println("Jpg file", dstFileName, "created")

}

//Take an existing png srcFileName and decode/encode it
func CreatePng() {
	srcFileName := "mouse.png"
	dstFileName := "newMouse.png"
	reader, err := os.Open(srcFileName)
	if err != nil {
		log.Fatal(err)
	}
	defer reader.Close()
	//Decode from reader to image format
	m, formatString, err := image.Decode(reader)
	if err != nil {
		log.Fatal(err)
		return
	}
	fmt.Println("Got format String", formatString)
	fmt.Println(m.Bounds())

	//Encode from image format to writer
	f, err := os.OpenFile(dstFileName, os.O_WRONLY|os.O_CREATE, 0777)
	if err != nil {
		log.Fatal(err)
		return
	}

	err = png.Encode(f, m)
	if err != nil {
		log.Fatal(err)
		return
	}
	fmt.Println("Png file", dstFileName, "created")

}

//Converts pre-existing base64 data (found in example of https://golang.org/pkg/image/#Decode) to test.png
func Base64toPng(data string, fileName string) {
	reader := base64.NewDecoder(base64.StdEncoding, strings.NewReader(data))
	m, formatString, err := image.Decode(reader)
	if err != nil {
		log.Fatal(err)
	}
	bounds := m.Bounds()
	fmt.Println(bounds, formatString)

	image_path := utils.LoadPathSaveImages()
	//Encode from image format to writer
	pngFilename := image_path + fileName + ".png"
	f, err := os.OpenFile(pngFilename, os.O_WRONLY|os.O_CREATE, 0777)
	if err != nil {
		log.Fatal(err)
		return
	}

	err = png.Encode(f, m)
	if err != nil {
		log.Fatal(err)
		return
	}
	fmt.Println("PNG file", pngFilename, "created")

}

//Given a base64 string of a JPEG, encodes it into an JPEG image test.jpg
func Base64toJpg(data string, fileName string) error {

	reader := base64.NewDecoder(base64.StdEncoding, strings.NewReader(data))
	m, formatString, err := image.Decode(reader)
	if err != nil {
		log.Fatal(err)
	}
	bounds := m.Bounds()
	fmt.Println("base64toJpg", bounds, formatString)

	image_path := utils.LoadPathSaveImages()
	//Encode from image format to writer
	pngFilename := image_path + fileName + ".jpg"
	f, err := os.OpenFile(pngFilename, os.O_WRONLY|os.O_CREATE, 0777)
	if err != nil {
		log.Fatal(err)
		return err
	}

	err = jpeg.Encode(f, m, &jpeg.Options{Quality: 100})
	if err != nil {
		log.Fatal(err)
		return err
	}

	fmt.Println("JPG file", pngFilename, "created")
	return nil

}

//Gets base64 string of an existing JPEG file
func GetJPEGbase64(fileName string) (string, error) {
	image_path := utils.LoadPathSaveImages()
	imgFile, err := os.Open(image_path + fileName + ".jpg")

	if err != nil {
		fmt.Println(err)
		return "DATA_NOT_EXIST_CODE", err
	}

	defer imgFile.Close()

	// create a new buffer base on file size
	fInfo, _ := imgFile.Stat()
	var size = fInfo.Size()
	buf := make([]byte, size)

	// read file content into buffer
	fReader := bufio.NewReader(imgFile)
	fReader.Read(buf)

	imgBase64Str := base64.StdEncoding.EncodeToString(buf)
	//fmt.Println("Base64 string is:", imgBase64Str)
	return imgBase64Str, nil

}

func CallAPIGetImageJapan(isbn string) (string, error) {
	var openDbResponse []OpenDbResponse
	resp, err := http.Get("https://api.openbd.jp/v1/get?isbn=" + isbn)
	if err != nil {
		log.Println(exterror.WrapExtError(err))
		return "", err
	}

	//We Read the response body on the line below.
	body, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		log.Println(exterror.WrapExtError(err))
		return "", err
	}

	err = json.Unmarshal(body, &openDbResponse)
	if err != nil {
		log.Println(exterror.WrapExtError(err))
		return "", err
	}

	cover_data := openDbResponse[0].Summary.Cover
	return cover_data, nil

}
