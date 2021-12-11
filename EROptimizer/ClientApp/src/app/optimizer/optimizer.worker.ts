/// <reference lib="webworker" />

import { IOptimizerWorkerRQ } from "./model/IOptimizerWorkerRQ";

addEventListener('message', (e: MessageEvent<IOptimizerWorkerRQ>) => {

  //const response = `worker response to ${e.data.key}`;
  //postMessage(response);
});
