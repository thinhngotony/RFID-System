import React, { useState, useEffect, useRef } from "react";

import socketIOClient from "socket.io-client";
import Carousels from "./components/Carousel";
import "bootstrap/dist/css/bootstrap.min.css";
import "./App.css";
import $ from 'jquery'; 
import { DisplayScreen } from "./components/Helpers";
import axios from 'axios';

function App() {
  const [data, setData] = useState([]);
  const [shelfTitle, setShelfTitle] = useState("");
  const [screen, setScreen] = useState(DisplayScreen.INIT);
  const [videoUrl, setVideoUrl] = useState("");
  const [dataWaitingContent, setDataWaitingContent] = useState("");
  const [noMessage, setNoMessage] = useState(false);
  const [indexWaiting, setIndexWaiting] = useState(-1);
  const [cCodeInfo, setCCodeInfo] = useState("");
  const socketRef = useRef();
  var rfidRef = useRef();
  var indexWaitingRef = useRef(0);
  var waitingScreenRef = useRef();
  var waitingTimeOutRef = useRef();
  var timeOut, hideTimeOut;
  
  const handleClick = () => {
    axios.post(process.env.REACT_APP_SERVER_SOCKET_HOST + "/api/rfid-scan", { "Rfid": rfidRef.current, "AutoReq": true });
  }


  useEffect(() => {
    if(waitingScreenRef.current === undefined) {
      return;
    }

    console.log("indexWaitingRef.current", indexWaitingRef.current);
    let type = waitingScreenRef.current[indexWaiting].type;
    let duration = waitingScreenRef.current[indexWaiting].duration;
    let param1 = waitingScreenRef.current[indexWaiting].param1;

    switch (type) {
      case "VIDEO":
        setDataWaitingContent(param1);
        setScreen(DisplayScreen.WAITING_VIDEO);
        break;
      case "WAITING":
        setDataWaitingContent("");
        setScreen(DisplayScreen.INIT);
        break;
      case "RFID":
        setDataWaitingContent("");
        axios.post(process.env.REACT_APP_SERVER_SOCKET_HOST + "/api/rfid-scan", { "Rfid": param1, "AutoReq": true });
        return; // do not set timeout here
      case "TEXT":
        setDataWaitingContent(param1);
        setScreen(DisplayScreen.TEXT);
        break;
      default:
        console.log("No screen type found");
    }

    waitingTimeOutRef.current = setTimeout(() => {
      indexWaitingRef.current = (indexWaitingRef.current+1) % Object.keys(waitingScreenRef.current).length;
      setIndexWaiting(indexWaitingRef.current);
    }, duration * 1000);
  }, [indexWaiting]);

  useEffect(() => {
    socketRef.current = socketIOClient.connect(process.env.REACT_APP_SERVER_SOCKET_HOST);
    
    //init data waiting screen
    socketRef.current.on("initData", (res) => {
      waitingScreenRef.current = res.reduce(function(obj, str, index) {
        obj[str["step"]] = {"type": str["type"], "duration": str["duration"], "param1": str["param1"]}; 
        return obj;
      }, {});
      setIndexWaiting(0);
    });

    //notify when rfid scan
    socketRef.current.on("displayScreen", (res) => {

      if(res.rfid != rfidRef.current ||  res.autoReq || res.type !== "CM") {
        if(res.autoReq || res.rfid != rfidRef.current){
          setData([]);
          setCCodeInfo('');
        }

        clearTimeout(waitingTimeOutRef.current);
        rfidRef.current = res.rfid;
        setVideoUrl('');
        setScreen(DisplayScreen[res.type]);
        setNoMessage(false);

        //data empty, then show message
        if(res.data?.length == 0) {
          clearTimeout(timeOut);
          setNoMessage(true);

          //hide message after 3s
          timeOut = setTimeout(() => {
            if (!res.autoReq && indexWaitingRef.current != 0) {
              setNoMessage(false);
              indexWaitingRef.current = 0;
              setIndexWaiting(0);
            } else{
              res.autoReq = false;
              indexWaitingRef.current = (indexWaitingRef.current+1) % Object.keys(waitingScreenRef.current).length;
              setIndexWaiting(indexWaitingRef.current);
            }
          }, 3000);

        } else {
          //data not empty, then show data after 1s
          setTimeout(() => {
            setCCodeInfo(res.cCodeInfo ?? "");
            setVideoUrl(res.videoUrl);
            setShelfTitle(res.title);
            setData(res.data);
            clearTimeout(timeOut);

            timeOut = setTimeout(() => {
              if(res.type == "CM"){
                setScreen(DisplayScreen.DIGITAL_SIGNAGE);
                clearTimeout(timeOut);

                timeOut = setTimeout(() => {

                  if (!res.autoReq && indexWaitingRef.current != 0) {
                    setData([]);
                    indexWaitingRef.current = 0;
                    setIndexWaiting(0);
                  } else{
                    res.autoReq = false;
                    indexWaitingRef.current = (indexWaitingRef.current+1) % Object.keys(waitingScreenRef.current).length;
                    setIndexWaiting(indexWaitingRef.current);
                  }
                }, process.env.REACT_APP_DISPLAY_SCREEN_TIMEOUT);

              } else if(res.type == "BOOK_INFO"){
                handleClick();
              } else { //case not CM, BOOK_INFO
                if (!res.autoReq && indexWaitingRef.current != 0) {
                  setData([]);
                  indexWaitingRef.current = 0;
                  setIndexWaiting(0);
                } else{
                  res.autoReq = false;
                  indexWaitingRef.current = (indexWaitingRef.current+1) % Object.keys(waitingScreenRef.current).length
                  setIndexWaiting(indexWaitingRef.current);
                }
              }
            }, res.type == "CM" ? 18000 : res.type == "BOOK_INFO" ? 10000 : process.env.REACT_APP_DISPLAY_SCREEN_TIMEOUT);
          }, (res.rfid == rfidRef.current && !res.autoReq && res.type === "DIGITAL_SIGNAGE") ? 0 : 1000);
        }
      }
    });

    window.addEventListener("load", () => {
      $(this).scrollTop(0);
      hideTimeOut = setTimeout(() => {
        $(".se-pre-con").hide();
      }, 4000);
    });

    window.addEventListener("beforeunload", () => {
      window.scrollTo(0, 0);
    });
    
    return () => {
      socketRef.current?.disconnect();
      rfidRef.current?.disconnect();
      indexWaitingRef.current?.disconnect();
      waitingScreenRef.current?.disconnect();
      waitingTimeOutRef.current?.disconnect();
      clearTimeout(timeOut);
      clearTimeout(hideTimeOut);
    };
  }, []);

  return (
    <div>
      <Carousels screen={screen} 
      title={shelfTitle} 
      data={data} 
      videoUrl={videoUrl} 
      noMessage={noMessage} 
      dataWaitingContent={dataWaitingContent} 
      cCodeInfo={cCodeInfo}
      handleClick={handleClick}/>
    </div>
  );
}

export default App;
