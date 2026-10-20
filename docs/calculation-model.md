# Calculation model

PowerModule Workbench uses a deliberately simplified educational model. It is
intended to demonstrate a traceable .NET engineering tool workflow and is not
validated for equipment design or certification.

## Inputs

| Input | Unit | Meaning |
|---|---:|---|
| DC-link voltage | V | DC voltage used for the current approximation |
| Output power | W | Requested output power |
| Switching frequency | Hz | Switching events per second |
| Ambient temperature | °C | Ambient thermal reference |
| Thermal resistance | °C/W | Simplified heat-transfer resistance |
| Conduction resistance | Ω | Simplified resistive loss parameter |
| Switching energy | J | Energy lost per switching event |
| Maximum junction temperature | °C | Configured thermal limit |

## Equations

```text
estimated RMS current = output power / DC-link voltage
conduction loss      = estimated RMS current² × conduction resistance
switching loss       = switching energy × switching frequency
total loss           = conduction loss + switching loss
junction temperature = ambient temperature + total loss × thermal resistance
efficiency           = output power / (output power + total loss) × 100
thermal margin       = maximum junction temperature - junction temperature
```

## Limitations

The model does not represent switching waveforms, semiconductor curves,
parasitics, cooling geometry, transient behavior, or validated device data.
Results are estimates for learning and portfolio demonstration only.
