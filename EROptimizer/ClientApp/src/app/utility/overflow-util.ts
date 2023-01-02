
export class OverflowUtil {

  static readonly HIDDEN_MARKER_ID = "overflow-min-height-handler-registered";
  static readonly ATTRIBUTE_NAME = "data-overflow-min-height";
  static readonly DEFAULT_MIN_HEIGHT = "400px";

  static setupOverflowMinHeight() {

    let hiddenMarkerEl: HTMLInputElement = document.getElementById(this.HIDDEN_MARKER_ID) as HTMLInputElement;
    if (hiddenMarkerEl == null) {
      hiddenMarkerEl = document.createElement("input") as HTMLInputElement;
      hiddenMarkerEl.id = this.HIDDEN_MARKER_ID;
      hiddenMarkerEl.type = "hidden";
      document.body.appendChild(hiddenMarkerEl);
      window.addEventListener("resize", OverflowUtil.resizeHandler.bind(this));
    }

    OverflowUtil.resizeHandler();
  }

  static resizeHandler() {
    //console.log("OverflowUtil.resizeHandler");
    let containers = document.querySelectorAll(`[${this.ATTRIBUTE_NAME}]`);
    console.log(containers);

    containers.forEach((c) => {

      let pixelHeightString = c.getAttribute(this.ATTRIBUTE_NAME);
      if (!pixelHeightString) pixelHeightString = this.DEFAULT_MIN_HEIGHT;
      let minHeight = parseInt(pixelHeightString.replace("px", ""));

      let el = c as HTMLElement;
      el.style.overflowY = "auto";
      //console.log(el.offsetHeight);

      if (el.offsetHeight >= minHeight) {
        //console.log("auto");
      } else {
        //console.log("default");
        el.style.overflowY = "visible";
      }

    });
  }
}
