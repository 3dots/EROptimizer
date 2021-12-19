import { IOptimizerResult, OptimizerResult } from "./OptimizerResult";

export enum OptimizerWorkerRSEnum {
  Progress = 0,
  Finished = 1
}

export class OptimizerWorkerRS implements IOptimizerWorkerRS {
  type!: OptimizerWorkerRSEnum;
  progress!: number;
  results!: OptimizerResult[];

  public constructor(init?: Partial<OptimizerWorkerRS>) {
    Object.assign(this, init);
  }
}

export interface IOptimizerWorkerRS {
  type: OptimizerWorkerRSEnum;
  progress: number;
  results: IOptimizerResult[];
}

