# **EV High-Voltage De-energization & Insulation Test — Simulation Narrative**

## **Educational Purpose**
This simulation trains students and service technicians to de-energize the high-voltage (HV) system of an electric vehicle and to verify the condition of its insulation. Participants work through the two-stage protocol used in HV service: first proving that the system carries no voltage, then measuring how well the HV lines are isolated from the vehicle body. The simulation also covers instrument validation, since a measuring device that reports a false zero is more dangerous than no measurement at all. Results are evaluated against the ECE R100 standard.

## **Simulation Flow — Step by Step**

### **Scene 1: Visual Inspection and Work Area Preparation**
The simulation begins in a repair shop with a lab car standing on a lift. Users inspect the HV battery before handling any tool. The check covers bolt and nut connections, damage to the housing, and the condition of the sockets and their seats. Burn marks, darkening around the sockets and fluid leakage are the main findings to look for, since darkening indicates arcing. If an abnormality is found, the vehicle is placed in quarantine and a specialist is notified instead of continuing. After the visual check is approved, the operation area is enclosed in the laboratory cage and the measurement points are made accessible.

Interactive elements:

- HV battery with inspectable bolts, housing and socket seats
- Quarantine or continue decision
- Laboratory cage and work area setup

### **Scene 2: Isolating the High-Voltage System**
The system is now shut down mechanically and electrically. Users pull the high-voltage manual service disconnect (MSD) out of its seat and fit the lock. The 12 V battery connection is then separated so the control system loses power as well. The instrument cluster is read at this point and the procedure branches. If the panel confirms that energy has been cut, work continues with the insulation test. If voltage is still shown, a stuck contactor is suspected and the manual absence-of-voltage test is carried out first.

Interactive elements:

- MSD socket with lock-out
- 12 V battery connector
- Dashboard readout with two outcomes

### **Scene 3: Instrument Verification**
Each instrument is proven before it is used. Users check the two-pole high-voltage tester physically: calibration date, display, probe mechanics and the ends of the cables. The tester is then applied to the 12 V battery terminals, red probe to positive and blue probe to negative. Reading 12 V confirms the device works.

The gigaohmmeter is checked next. Its selector is set one step above the pack voltage, for example 500 V for a 400 V battery. The stop button is pressed so the device discharges itself, then the two probes are touched together. A reading of 0.000 ohm means the instrument is ready.

Interactive elements:

- Two-pole HV tester with red and blue probes
- 12 V battery terminals used as a reference source
- Gigaohmmeter with voltage selector, stop button and display

### **Scene 4: Absence-of-Voltage Test**
The intermediate measuring unit, the black box that connects the battery to the instruments, is checked before it carries a measurement. Users inspect it physically, then set the multimeter to its ohm range and measure the resistance across its HV+, SCR and HV− pins. A break inside this adapter would produce a 0 V reading while the system is still live, so the check is not optional. The two sockets on either side of the unit are connected to each other once it passes.

The unit is then plugged into the main socket of the HV battery, and the probes of the high-voltage tester are inserted into its HV+ and HV− terminals. Users read the display and confirm that no energy remains in the system before moving on.

Interactive elements:

- Intermediate measuring unit with HV+, SCR and HV− pins
- Multimeter in ohm mode
- Detachable sockets on both sides of the unit
- Voltage readout with pass and fail states

### **Scene 5: Insulation Resistance Measurement**
With the gigaohmmeter set above the pack voltage, users place one probe into the HV+ hole of the intermediate unit and hold the other against bare, unpainted metal on the chassis. The test button is pressed and the resistance is recorded, for example 50 MΩ. The probe is then moved from HV+ to HV− while the chassis probe stays in place, and the measurement is repeated, for example 60 MΩ.

If the vehicle carries additional HV components such as an air conditioning compressor, each one is connected to the intermediate unit through its matching socket and tested separately. All values are compared against ECE R100, which requires at least 500 ohm per volt. For a 400 V system the reading must be well above 200 kΩ; a healthy system usually measures in the megaohm or gigaohm range.

Interactive elements:

- Gigaohmmeter probes and the HV+ / HV− holes
- Chassis contact point on bare metal
- Test button, resistance display and measurement record
- Sockets for additional components

### **Scene 6: Battery Removal and Reporting**
Once the system is confirmed dead and properly isolated, the vehicle is raised on the lift. Users release the HV connectors on the battery one at a time, then raise the carrier tray under the pack until it takes the weight. The battery is moved to the marked safe zone. The recorded resistance values are collected into the report, compliance with the standard is confirmed, and the operation is closed.

Interactive elements:

- Lift control panel
- HV connectors released by grip and pull
- Carrier tray with its own control
- Safe drop zone and completion report

## **Learning Outcomes**
This simulation enables learners to:

- Apply the two-stage HV protocol: prove the system is de-energized, then prove it is insulated.
- Carry out a visual risk inspection and recognise the findings that require quarantine.
- Remove and lock the manual service disconnect and separate the 12 V supply in the correct order.
- Validate a high-voltage tester and a gigaohmmeter before relying on their readings.
- Check the internal continuity of an intermediate measuring unit and explain why a false zero-volt reading is dangerous.
- Measure HV+ and HV− insulation resistance against the chassis and evaluate the results against ECE R100.
- Remove an HV battery using a lift and carrier tray, and report the measured values.

## **Reflection and Discussion Questions**
1. Why is the dashboard reading treated as a branch point rather than a final result, and what fault does remaining voltage suggest?
2. What could happen if the intermediate measuring unit were used without checking its continuity first?
3. Why is the high-voltage tester validated on a known 12 V source before it is used on the HV system?
4. Why must the gigaohmmeter be set above the battery voltage, and what does the short-circuit check prove?
5. A 400 V system measures 180 kΩ between HV+ and chassis. Does this meet ECE R100, and what should be done next?
6. Why must the chassis probe touch bare metal, and how would a painted contact point affect the reading?
