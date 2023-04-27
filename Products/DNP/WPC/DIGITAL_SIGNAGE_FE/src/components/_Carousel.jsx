import React, { Component} from "react";
import BookSell from "./BookSell/BookSell";
import NoBook from "./NoBook/NoBook";
import BookOnShelf from "./BookOnShelf/BookOnShelf";
import BookOnTable from "./BookOnTable/BookOnTable";
import NoBookOnTable from "./BookOnTable/NoBookOnTable";
import Carousel from "react-bootstrap/Carousel";
import {Tomm} from "./Helpers";
import { DisplayScreen } from "./Helpers";
import $ from 'jquery'; 

class Carousels extends Component {
  constructor(props) {
    super(props);
    this.state = {
      screen: this.props.screen,
      title: this.props.title,
      data: this.props.data,
      videoUrl: this.props.videoUrl,
      dataWaitingContent: this.props.dataWaitingContent,
      noMessage: this.props.noMessage,
      cCodeInfo: this.props.cCodeInforeact,
      handleClick: this.props.handleClick
    };
  }

  static getDerivedStateFromProps(props, state) {
    var myVideo = $("#myVideo")[0];
    var waitingVideo = $("#waitingVideo")[0];

    if (props.screen === DisplayScreen.CM) {
      waitingVideo.pause();
      myVideo.load();
      myVideo.play();
      var myVideoPromise = myVideo.play();

      if (myVideoPromise !== undefined) {
        myVideoPromise.then(_ => {
        })
          .catch(error => {
          });
      }
    }
    else if (props.screen === DisplayScreen.WAITING_VIDEO) {
      myVideo.pause();
      waitingVideo.load();
      waitingVideo.play();
      var waitingVideoPromise = waitingVideo.play();

      if (waitingVideoPromise !== undefined) {
        waitingVideoPromise.then(_ => {
        })
          .catch(error => {
          });
      }
    }
    else {
      myVideo?.pause();
      waitingVideo?.pause();
    }

    return { screen: props.screen, 
      title: props.title, 
      data: props.data, 
      videoUrl: props.videoUrl, 
      dataWaitingContent: props.dataWaitingContent, 
      noMessage: props.noMessage, 
      cCodeInfo: props.cCodeInfo,
      handleClick: props.handleClick };
  }
  
  render() {
    return (
      <div>
        <div className="se-pre-con">
          <p className="xxx-large-normal" style={{color: 'gray'}}>Loading</p>
        </div>
        <div className="container-fluid p-0 m-0">
          <div className="w-100" id="cf4">
          <Carousel
            activeIndex={this.state.screen}
            nextIcon={<p></p>}
            prevIcon={<p></p>}
            indicators={false}
            fade = {true}
          >
            {/* Screen init */}
            <Carousel.Item key={0} style={{transitionDuration: `0.5s`}}>
              <div id="cf3"/>
              <Carousel.Caption>
                <div className="h-full" style={{ paddingTop: `44vh` }}>
                  <p className="xxxx-large">店内のお好きな本を</p>
                  <p className="xxxx-large">かざしてください。</p>
                  <p className="xxxx-large">ほかにも人気の本を</p>
                  <p className="xxxx-large">見つけられます。</p>
                </div>
                <div className="d-flex justify-content-center align-items-end" style={{ marginTop: `-100px`}}>
                  <img className="w-13" src="../parts/arrow01.png" alt="" style={{ maxWidth: `30vw`}}/>
                </div>
              </Carousel.Caption>
            </Carousel.Item>
            {/* Screen top 10 best seller */}
            <Carousel.Item key={1} style={{transitionDuration: `0.5s`}}>
              <div id="cf3" style={{backgroundColor: `black`, opacity: `30%`}}/>
              <Carousel.Caption>
              {this.state.data.length > 0 &&
                <div className="container px-0">
                  <p className="xxx-large">おすすめの全国売上TOP10</p>
                  <div className="row row-cols-3 d-flex justify-content-center">
                  <BookSell classBind={"top-1"}
                    top={"../parts/rank01.png"}
                    rank={"第" + this.state.data[0]?.rank + "位"}
                    img={this.state.data[0]?.img || "../parts/noimage.png"}
                    name={this.state.data[0]?.name}
                    qrcode={this.state.data[0]?.qrcode} />
                    <div style={{position: "absolute", marginLeft: "58vw", marginTop: "2vh", textAlign: "left"}} dangerouslySetInnerHTML={{ __html: this.state.cCodeInfo }}></div>
                  </div>
                  <div className="row row-cols-3 justify-content-center">
                    {this.state.data.map((obj, index) => {
                      if (index !== 0) {
                        return (
                          <BookSell key={index}
                            isHidden={index > 2 ? "isHidden" : ''}
                            classBind={index === 2 ? "top-3" : ''}
                            top={index < 3 ? (index === 1 ? "../parts/rank02.png" : "../parts/rank03.png") : ''}
                            rank={"第" + obj?.rank + "位"}
                            img={obj?.img || "../parts/noimage.png"}
                            name={obj?.name}
                            qrcode={obj?.qrcode} />
                        );
                      }
                      return null;
                    })}
                  </div>
                  <div className="h-full" style={{marginTop: '2vh'}}>
                    <p className="xxx-large-normal">honto withアプリでバーコードをスキャンすると</p>
                    <p className="xxx-large-normal">その本の詳細情報がスマホに表示されます。</p>
                  </div>
                </div>
              }
              {this.state.noMessage == true && 
                <div className="h-full" style={{ paddingTop: `44vh` }}>
                  <p className="xxx-large">該当する全国売上TOP10のデータはありません。</p>
              </div>
              }
              </Carousel.Caption>
            </Carousel.Item>
             {/* Screen smart shelf */}
            <Carousel.Item key={2} style={{transitionDuration: `0.5s`}}>
              <div id="cf3"/>
              <Carousel.Caption className="d-flex justify-content-center align-items-end" style={{
                top: `6%`
              }}>
                <div style={{ position: `absolute`, top: `-3.5vh` }} className="xxx-large">{this.state.title}</div>
              {this.state.data.length > 0 &&
                <div className="shelf container d-flex justify-content-center" style={{
                  background: `url("../parts/shelf01.png")`, 
                  backgroundRepeat:  `no-repeat`,
                  backgroundSize: `100% 100%`,
                  backgroundPosition: `center`,
                  }}>
                  <div className={`row row-cols-${process.env.REACT_APP_SHELF_COL_SIZE} inner-container `}
                    style={{
                      width: `91%`
                    }}>
                    {this.state.data.map((obj, index) => {
                      if (obj) {
                        return (
                          <BookOnShelf
                            key = {index}
                            rank={obj.rank}
                            view_time_in_seconds={Tomm(obj.view_time_in_seconds)}
                            view_cnt={obj.view_cnt + "回"}
                            name={obj.name}
                            img={obj.img} />
                        );
                      }
                      else {
                        return (<NoBook key={index}/>)
                      }
                    })}
                  </div>
                </div>
              }
              </Carousel.Caption>
            </Carousel.Item>
            {/* Screen smart table */}
            <Carousel.Item key={3} style={{transitionDuration: `0.5s`}}>
              <div id="cf3" style={{ minHeight: `85.9vh`}}/>
              <Carousel.Caption className="d-flex justify-content-center align-items-end" style={{
                top: `6%`,
                height: `82%`
              }}>
                <div style={{ position: `absolute`, top: `-3.5vh` }} className="xxx-large">{this.state.title}</div>
                <div className="smart-table container d-flex justify-content-center" style={{
                  backgroundColor: `gray`,
                  paddingTop: `1vh`,
                  }}>
                  <div className="row row-cols-4 inner-container "
                    style={{
                      width: `100%`
                    }}>
                    {this.state.data.map((obj, index) => {
                      if (obj) {
                        return (
                          <BookOnTable
                            key = {index}
                            rank={obj.rank}
                            view_time_in_seconds={Tomm(obj.view_time_in_seconds)}
                            view_cnt={obj.view_cnt + "回"}
                            name={obj.name}
                            img={obj.img} />
                        );
                      }
                      else {
                        return (<NoBookOnTable key={index}/>)
                      }
                    })}
                  </div>
                </div>
              </Carousel.Caption>
              <div className="d-flex justify-content-center align-items-end">
                <img className="w-13" src="../parts/arrow01.png" alt="" style={{ maxWidth: `30vw`}}/>
              </div>
            </Carousel.Item>
            {/* Screen CM */}
            <Carousel.Item key={4} style={{transitionDuration: `0.5s`}}>
              <div id="cf3"/>
              <Carousel.Caption className="d-flex align-items-center justify-content-center" style={{
                  height: `100%`,
                  }}>
                <div className="smart-table container d-flex align-items-center justify-content-center" style={{
                  
                  }}>
                  <div className="row row-cols-4 inner-container "
                    style={{
                      width: `100%`
                    }}>
                    <video id="myVideo">
                      <source src={this.state.videoUrl ? this.state.videoUrl[0]?.dcv_video_url : ""} type="video/mp4"/>
                    </video>
                  </div>
                </div>
              </Carousel.Caption>
              <div className="d-flex justify-content-center align-items-end">
                <img className="w-13" src="../parts/arrow01.png" alt="" style={{ maxWidth: `30vw`}}/>
              </div>
            </Carousel.Item>
            {/* Waiting Screen Video */}
            <Carousel.Item key={5} style={{transitionDuration: `0.5s`}}>
              <div id="cf3"/>
              <Carousel.Caption className="d-flex align-items-center justify-content-center" style={{
                  height: `100%`,
                  }}>
                <div className="smart-table container d-flex align-items-center justify-content-center" style={{
                  
                  }}>
                  <div className="row row-cols-4 inner-container "
                    style={{
                      width: `100%`
                    }}>
                    <video id="waitingVideo">
                      <source src={this.state.dataWaitingContent} type="video/mp4"/>
                    </video>
                  </div>
                </div>
              </Carousel.Caption>
              <div className="d-flex justify-content-center align-items-end">
                <img className="w-13" src="../parts/arrow01.png" alt="" style={{ maxWidth: `30vw`}}/>
              </div>
            </Carousel.Item>
            {/* Screen text */}
            <Carousel.Item key={6} style={{transitionDuration: `1.5s`}}>
              <div id="cf3"/>
              <Carousel.Caption>
                  <div className="h-full" style={{ paddingTop: "5vh", position: "relative" }}>
                    <div style={{
                      paddingLeft: "10vw", 
                      paddingRight: "10vw", 
                      whiteSpace: "normal", 
                      fontSize: "2.5vh",
                      }} className="waiting-text-div" dangerouslySetInnerHTML={{ __html: this.state.dataWaitingContent }}></div>
                  </div>
              </Carousel.Caption>
            </Carousel.Item>
            {/* Screen book info */}
            <Carousel.Item key={7} style={{transitionDuration: `0.5s`}}>
              <div id="cf3" style={{backgroundColor: `black`, opacity: `30%`}}/>
              <Carousel.Caption>
              {this.state.data.length > 0 && this.state.data[0] &&
                <div className="container px-3 xx-large-normal">
                  <p className="xxx-large">商品情報</p>
                  <div className="row" style={{paddingTop: "3vh", margin: "0vw"}}>
                    <div className="col-8" style={{textAlign: "left"}}>
                    <table className="table-ellipsis">
                      <tbody>
                        <tr>
                          <td>商品名</td>
                          <td>{this.state.data[0].bqgm_goods_name}</td>
                        </tr>
                        <tr>
                          <td>出版社</td>
                          <td>{this.state.data[0].bqgm_s_maker_name}</td>
                        </tr>
                        <tr>
                          <td>著者名</td>
                          <td>{this.state.data[0].bqgm_s_artist_name}</td>
                        </tr>
                        <tr>
                          <td>トーハン分類</td>
                          <td>{this.state.data[0].bqmt_btsb_name}・{this.state.data[0].bqmt_mtsb_name}・{this.state.data[0].bqmt_stsb_name}</td>
                        </tr>
                        <tr>
                          <td>ＩＳＢＮＣコード</td>
                          <td>{this.state.data[0].bqgm_c_code}</td>
                        </tr>
                        <tr>
                          <td>版型</td>
                          <td>{this.state.data[0].bqgm_hangata}</td>
                        </tr>
                        <tr>
                          <td>発売予定⽇</td>
                          <td>{this.state.data[0].bqgm_sales_date}</td>
                        </tr>
                        <tr>
                          <td>シリーズ名</td>
                          <td>{this.state.data[0].serial}</td>
                        </tr>
                        <tr>
                          <td>価格</td>
                          <td>{this.state.data[0].bqgm_price}</td>
                        </tr>
                      </tbody>
                    </table>
                    </div>
                    <div className="col-4">
                      {this.state.data[0].bqgm_image == "" && <div style={{width: "80%", height: "80%", border: "2px solid white", margin: "10%"}}></div>}
                      {this.state.data[0].bqgm_image != "" && <img src={this.state.data[0].bqgm_image} alt="" style={{width: "100%"}}/>}
                    </div>
                  </div>
                  <div className="row border-white">
                    <div>
                      商品説明
                    </div>
                    <div className="overflow-hidden-scroll" style={{height: "20vh"}}>
                      {this.state.data[0].text_content}
                    </div>
                  </div>
                  <div className="row border-white">
                    <div>
                      著者紹介
                    </div>
                    <div className="overflow-hidden-scroll" style={{height: "17vh"}}>
                    {this.state.data[0].person_name}
                    <div>
                      著者紹介
                    </div>
                    {this.state.data[0].biographical_note}
                    </div>
                  </div>
                  <div className="row" style={{marginTop: "3vh"}}>
                    <p className="btn btn-success xxx-large-normal" onClick={this.state.handleClick}>「この本の関連情報が次に表示されます」</p>
                  </div>
                </div>
              }
              </Carousel.Caption>
            </Carousel.Item>
          </Carousel>
          </div>
        </div>
      </div>
    );
  }
}

export default Carousels;
