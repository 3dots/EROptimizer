import { IOptimizerResult } from "./IOptimizerResult";

export enum OptimizerWorkerRSEnum {
  Progress = 0,
  Finished = 1
}

export class OptimizerWorkerRS implements IOptimizerWorkerRS {
  type!: OptimizerWorkerRSEnum;
  progress!: number;
  finishedResponse!: IOptimizerWorkerFinished;

  public constructor(init?: Partial<OptimizerWorkerRS>) {
    Object.assign(this, init);
  }
}

export interface IOptimizerWorkerRS {
  type: OptimizerWorkerRSEnum;
  progress: number;
  finishedResponse: IOptimizerWorkerFinished;
}

export interface IOptimizerWorkerFinished {
  results: IOptimizerResult[];
}
