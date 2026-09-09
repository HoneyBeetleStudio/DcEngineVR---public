# **DC Motor Construction & Operating Principle — Simulation Narrative**

## **Educational Purpose**
This simulation teaches the physical construction of a DC motor and the way electrical energy is converted into mechanical rotation. Learners move between full-scale motors in a workshop, examine each component separately, follow the current along its path through the machine, and compare a basic DC motor with a closed-loop brushed DC servo. By the end of the session a learner should be able to name every major part, describe its function, and explain what a servo adds to a plain motor.

## **Simulation Flow — Step by Step**

### **Scene 1: Orientation at the Motor Workstation**
The simulation opens in a workshop with three motor stations set up on benches: a basic DC motor, a cut-away DC motor, and a brushed DC servo. An information panel introduces the subject. The DC motor is one of the fundamental components of electromechanical systems that convert electrical energy into mechanical energy, and its operating principle is based on the torque produced by the magnetic force (Lorentz force) acting on a current-carrying conductor placed in a magnetic field. Users can approach any station in any order. Panel text and narration change according to the motor in front of them.

Interactive elements:

- Three motor stations with their own title and description panels
- Narration and panel text triggered by proximity
- Free movement between benches

### **Scene 2: Component Tour**
At the DC Motor Parts station the motor is shown as an exploded assembly with nine labelled components:

1. Frame (Housing)
2. Stator
3. Rotor (Armature)
4. Armature Windings
5. Commutator
6. Brushes
7. Shaft
8. Bearings
9. End Covers (End Bells)

Selecting a label highlights the matching part on the model and opens a short description. Learners can view each part from any angle before continuing. The tour groups the components into the stationary magnetic circuit (frame, stator), the rotating assembly (rotor, windings, commutator, shaft) and the parts that connect the two (brushes, bearings).

Interactive elements:

- Nine selectable part labels with highlight on select
- Exploded and assembled view toggle
- Description popup for each part

### **Scene 3: How a DC Motor Works**
The simulation then traces the energy path from end to end, supported by video and by highlights that follow the current through the machine. Current enters through the brushes, passes through the commutator and into the armature windings. The magnetic field of the stator interacts with the field in the rotor. This interaction produces torque and the rotor begins to turn. The rotor turns the shaft, and the shaft delivers mechanical energy to the load. The commutator reverses the direction of current in the windings as the rotor turns, which is what keeps the torque acting in one direction.

Interactive elements:

- Step-by-step panels with next and previous navigation
- Video player with play, pause and scrub
- Animated current path from brushes to shaft

### **Scene 4: Powering the Motor**
Users connect the motor to a supply. A cable is picked up and its connector is plugged into the motor terminals, and the result is shown immediately: current flows, the field builds and the shaft turns. Swapping the polarity reverses the direction of rotation.

Interactive elements:

- Grabbable cable with snap-in connector
- Motor terminals with polarity marking
- Rotation response and direction change on reversed polarity

### **Scene 5: Basic Motor and Brushed DC Servo**
The last station sets the basic motor against a closed-loop system. Basic DC motors convert direct current into continuous mechanical rotation. Servo motors provide high-precision control of angular or linear position, velocity and acceleration. A brushed DC servo uses the same brush and commutator construction, but a controller compares the commanded signal with the actual position reported by an encoder and works to remove the difference. Learners set a target position and watch the servo settle on it, then run the basic motor and see that it only spins, with nothing closing the loop.

Interactive elements:

- Brushed DC servo station with encoder feedback display
- Target position input and live error readout
- Side-by-side comparison with the basic DC motor

### **Scene 6: Knowledge Check**
The session closes with a short assessment. Learners label the nine components on an unmarked motor, place the stages of the current path in the correct order, and decide whether a described system is open-loop or closed-loop. The results appear on the completion panel.

Interactive elements:

- Drag and drop labelling task
- Current path ordering task
- Completion summary

## **Learning Outcomes**
This simulation enables learners to:

- Identify the nine major components of a DC motor and state the function of each.
- Explain the Lorentz force principle that produces torque in a current-carrying conductor in a magnetic field.
- Follow the current path from the brushes through the commutator and windings to the shaft and the load.
- Describe how the commutator keeps torque acting in one direction.
- Predict the effect of reversed supply polarity on the direction of rotation.
- Distinguish an open-loop DC motor from a closed-loop brushed DC servo and explain the function of the encoder.

## **Reflection and Discussion Questions**
1. Why is the commutator necessary, and what would happen to the rotor if it were removed?
2. Which components form the stationary magnetic circuit and which form the rotating assembly?
3. How does the interaction between the stator field and the rotor field produce torque?
4. Reversing the supply polarity reverses the rotation. Why does this follow from the Lorentz force principle?
5. What does the encoder in a brushed DC servo measure, and how is that value used by the controller?
6. Brushes and bearings both wear over time. What symptoms would each produce, and how would you tell them apart?
