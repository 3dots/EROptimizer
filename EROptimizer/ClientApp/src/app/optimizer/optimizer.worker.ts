/// <reference lib="webworker" />

import { IArmorDataDto } from "../../service/dto/IArmorDataDto";
import { IArmorPieceDto } from "../../service/dto/IArmorPieceDto";
import { ArmorCombo } from "./model/ArmorCombo";
import { ConfigTypeEnum, OptimizerConfigDto } from "./model/OptimizerConfigDto";
import { OptimizerResult } from "./model/OptimizerResult";
import { IOptimizerWorkerRQ } from "./model/OptimizerWorkerRQ";
import { OptimizerWorkerRS, OptimizerWorkerRSEnum } from "./model/OptimizerWorkerRS";

addEventListener('message', (e: MessageEvent<IOptimizerWorkerRQ>) => {

  //console.log("Worker recieved message:");
  //console.log(e.data);

  let data: IArmorDataDto = e.data.data;
  let config: OptimizerConfigDto = new OptimizerConfigDto(e.data.config);

  let totalAvailableWeight = config.totalAvailableWeightCalc;

  //console.log(totalAvailableWeight);

  let results: ArmorCombo[] = [];

  for (let i = 0; i < data.head.length; i++) {
    if (i > 0) postMessage(new OptimizerWorkerRS({ type: OptimizerWorkerRSEnum.Progress, progress: 100 * i / data.head.length, workerIndex: e.data.workerIndex }));

    let head: IArmorPieceDto = data.head[i];

    let weight = head.weight;
    if (weight > totalAvailableWeight) continue;

    for (let j = 0; j < data.chest.length; j++) {
      let chest: IArmorPieceDto = data.chest[j];

      weight = head.weight + chest.weight;
      if (weight > totalAvailableWeight) continue;

      for (let k = 0; k < data.gauntlets.length; k++) {
        let gauntlets: IArmorPieceDto = data.gauntlets[k];

        weight = head.weight + chest.weight + gauntlets.weight;
        if (weight > totalAvailableWeight) continue;

        for (let l = 0; l < data.legs.length; l++) {
          let legs: IArmorPieceDto = data.legs[l];

          weight = head.weight + chest.weight + gauntlets.weight + legs.weight;
          if (weight > totalAvailableWeight) continue;

          let combo = new ArmorCombo(head, chest, gauntlets, legs, config, data.talismans);

          if (combo.avgPhysical < config.minAvgPhysical ||
            combo.physical < config.minPhysical ||
            combo.physicalStrike < config.minPhysicalStrike ||
            combo.physicalSlash < config.minPhysicalSlash ||
            combo.physicalPierce < config.minPhysicalPierce ||
            combo.magic < config.minMagic ||
            combo.fire < config.minFire ||
            combo.lightning < config.minLightning ||
            combo.holy < config.minHoly ||
            combo.immunity < config.minImmunity ||
            combo.robustness < config.minRobustness ||
            combo.focus < config.minFocus ||
            combo.vitality < config.minVitality ||
            combo.poise < config.minPoise)
              continue;

          if (results.length == 0) {
            results.push(combo);
          } else if (results.length == config.numberOfResults && combo.score <= results[results.length - 1].score) {
            continue;
          } else {

            let inserted = false;

            for (let m = 0; m < results.length; m++) {
              let item: ArmorCombo = results[m];
              if (item.score < combo.score) {
                //Push all items down, insert at m.
                results.splice(m, 0, combo);
                inserted = true;

                //Remove last item if necessary
                if (results.length > config.numberOfResults) results.splice(results.length - 1, 1);

                break;
              }
            }

            if (results.length < config.numberOfResults && !inserted) {
              results.push(combo);
            }
          }
        }
      }

    }
  }

  postMessage(new OptimizerWorkerRS({
    type: OptimizerWorkerRSEnum.Finished,
    results: results.map((x) => new OptimizerResult(x)),
    workerIndex: e.data.workerIndex
  }))

});
