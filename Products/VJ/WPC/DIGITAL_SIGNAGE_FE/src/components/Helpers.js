export const DisplayScreen = {
    "INIT" : 0,
    "DIGITAL_SIGNAGE" : 1,
    "SMART_SHELF" : 2,
    "SMART_TABLE" : 3,
    "CM" : 4,
    "WAITING_VIDEO" : 5,
    "TEXT" : 6,
  }

export function Tomm(secs) {
  return (Math.round(secs / 60 )) + "分";
}