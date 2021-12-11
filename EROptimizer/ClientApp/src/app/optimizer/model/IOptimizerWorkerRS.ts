
export enum OptimizerWorkerRSEnum {
  Progress = 0,
  Finished = 1
}

export interface IOptimizerWorkerRS {
  type: OptimizerWorkerRSEnum;
  progress: number;
  finishedResponse: IOptimizerWorkerFinished;
}

export interface IOptimizerWorkerFinished {

}
