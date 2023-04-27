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

  // let screenData = process.env.REACT_APP_WAITING_SCREEN.split("|").reduce(function(obj, str, index) {
  //   let strParts = str.split(",");
  //   if (strParts[0] && strParts[1]) {
  //     obj[strParts[0]] = {"type": strParts[1], "duration": strParts[2], "param1": strParts[3]}; 
  //   }
  //   return obj;
  // }, {});

  useEffect(() => {
    if(waitingScreenRef.current === undefined) {
      return;
    }
    console.log("screenData", waitingScreenRef.current);

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
        return;
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
      console.log("waitingVideoRef.current 61: ", waitingScreenRef.current[indexWaitingRef.current]);
      console.log("next indexWaiting 62: ", indexWaitingRef.current);
    }, duration * 1000);
  }, [indexWaiting]);

  useEffect(() => {
    socketRef.current = socketIOClient.connect(process.env.REACT_APP_SERVER_SOCKET_HOST);
    
    socketRef.current.on("initData", (res) => {
      console.log("initData 71: ", res);
      waitingScreenRef.current = res.reduce(function(obj, str, index) {
        obj[str["step"]] = {"type": str["type"], "duration": str["duration"], "param1": str["param1"]}; 
        return obj;
      }, {});
      setIndexWaiting(0);
    });

    socketRef.current.on("displayScreen", (res) => {
      console.log("displayScreen", res.autoReq);
      if(res.rfid != rfidRef.current ||  res.autoReq) {
        clearTimeout(waitingTimeOutRef.current);
        rfidRef.current = res.rfid;
        setData([]);
        setVideoUrl('');
        setCCodeInfo('');
        setScreen(DisplayScreen[res.type]);
        setNoMessage(false);
        //data empty, then show message
        if(res.data?.length == 0) {
          clearTimeout(timeOut);
          setNoMessage(true);
          timeOut = setTimeout(() => {
            console.log("autoReq" + res.autoReq);
            if (!res.autoReq) {
              setNoMessage(false);
              //setScreen(DisplayScreen.INIT);
              indexWaitingRef.current = 0;
              setIndexWaiting(0);
            } else{
              res.autoReq = false;
              indexWaitingRef.current = (indexWaitingRef.current+1) % Object.keys(waitingScreenRef.current).length;
              setIndexWaiting(indexWaitingRef.current);
              console.log("indexWaiting 91: " + indexWaitingRef.current);
              console.log("indexWaiting type 92: " + waitingScreenRef.current[indexWaitingRef.current].type);
            }
          }, 3000);
        } else {
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
                  console.log("autoReq" + res.autoReq);
                  if (!res.autoReq) {
                    //setScreen(DisplayScreen.INIT);
                    setData([]);
                    indexWaitingRef.current = 0;
                    setIndexWaiting(0);
                  } else{
                    res.autoReq = false;
                    indexWaitingRef.current = (indexWaitingRef.current+1) % Object.keys(waitingScreenRef.current).length;
                    setIndexWaiting(indexWaitingRef.current);
                    console.log("indexWaiting 114: " + indexWaitingRef.current);
                    console.log("indexWaiting type 115: " + waitingScreenRef.current[indexWaitingRef.current].type);
                  }
                }, process.env.REACT_APP_DISPLAY_SCREEN_TIMEOUT);
              } else { //case not CM
                console.log("autoReq" + res.autoReq);
                if (!res.autoReq) {
                  //setScreen(DisplayScreen.INIT);
                  setData([]);
                  indexWaitingRef.current = 0;
                  setIndexWaiting(0);
                } else{
                  res.autoReq = false;
                  indexWaitingRef.current = (indexWaitingRef.current+1) % Object.keys(waitingScreenRef.current).length
                  setIndexWaiting(indexWaitingRef.current);
                  console.log("indexWaiting next 127: " + ((indexWaitingRef.current+1) % Object.keys(waitingScreenRef.current).length));
                  console.log("indexWaiting 128: " + indexWaitingRef.current);
                  console.log("indexWaiting type 129: " + waitingScreenRef.current[indexWaitingRef.current].type);
                }
              }
            }, res.type == "CM" ? 18000 : process.env.REACT_APP_DISPLAY_SCREEN_TIMEOUT);
          }, 1000);
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
      <Carousels screen={screen} title={shelfTitle} data={data} videoUrl={videoUrl} noMessage={noMessage} dataWaitingContent={dataWaitingContent} cCodeInfo={cCodeInfo}/>
    </div>
  );
}

export default App;
