# inputRecordingPlayback

Presentation made for explaining the intricacies of the project, hurdles faced and overcome as well as different features and cases accounted for : https://docs.google.com/presentation/d/1irv3XbqOCKwwiFUlZ4S4b9p7bleJeUIsk02jsiNsn4o/edit?usp=sharing

A demo of the project showing: 
1. Position,rotation,scale values being tracked and changes replayed
2. Instantiation and enabling of objects is tracked(the two cubes appearing at 0:05 in the video below)
3. To optimize efficiency, only those materials values were tracked that changed, when multiple materials were present on an object. (Materials of cube changing to blue 0:07)
4. Trail renderer being enabled on a gameObject is tracked (0:12)
5. Changes in a parent are also tracked (at 0:16 notice how position of larger cube changes, that is to show changing of parent to another tracked parent)
6. Line renderer being enabled between two points, then those two points are also tracked. (0:18)

Demo of the video:
https://github.com/himanchalsharmaa/inputRecordingPlayback/assets/95272385/a66523fd-a518-402a-816c-be4b2effa95b

