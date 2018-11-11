# labJuiceWin
LabJack control application. For Windows.

## What is?!
This is a C# WinForm application that will allow you to communicate with the [LabJack U12 acquisition board](https://labjack.com/). It will give you a visual overview of the inputs waveform in a graph and allow you to calculate the Fast Fourier Transform (FFT) from that input. 

This application also have a demo visualization for sinus waves. A LabJack acquisition board is not required to use the demo tool.


<p align="center"><img src="https://github.com/Stickano/labJuiceWin/blob/master/Capture.PNG" /></p>

## How-to?
When you compile this application you will be presented with two graphs on the left side and some controls on your right side. 
* **Graphs:**
  * Top graph: Sinusoidal waveform, second & third harmonic.
  * Bottom Graph: Frequency Hz/Magnitude.
* **Controls:**
  * A textbox that will update with values as you use the controls.
  * Register LabJack U12 acquisition board.
    * Make sure your device is connected to the USB port.
  * Capture Data.
    * Once the device is registered, record and plot the Analog Input data into the sinus graph. 
  * Calculate FFT.
    * Calculate the Fast Fourier Transform (Hz/Magnitude) from the waveform values (waveform, second & third harmonic).
  * **Demo tools:**
    * Simulate a waveform with second and third harmonic (optional).
    * Control Hz for all three waveform.
    * Control the magnitude for second and third harmonic.
    * Enable/Display plot of second and third harmonic.
    * Control Phase angel for second and third harmonic (360°).

## TODO:
  * [ ] This whole codebase is a major mess and needs a good ol' refactoring before anything else..
  * [ ] Record Analog Output values (send to Output).
  * [ ] Not quite sure if Digital I/O works. Should probably test.
  * [ ] Phase angel not really working. 
  * [x] Read Analog Input into graph.
  * [x] Calculate FFT (Hz/Magnitude) from sinusoidal waveform, second & third harmonic (Frequenzy, Magnitude & Phase angle).
  * [x] Demo tools to generate and plot sinus waves.
  
## LabJack libraries
1. LabJack's U12 library driver can be found on [their own website.](https://labjack.com/support/software/installers/u12)
1. They have an [example function project](https://labjack.com/support/software/examples/u12/dotnet) available also (LabJackDotNet.zip).
  
## References
#### Based on
This is very much based on [this youtube serie](https://www.youtube.com/watch?v=jUyTk0Jw0MI), where the author i.e. talks about the FFT and communicating with your U12 device. 

#### LabJuice
This repository is called labJuice*Win* because I already tried to [play with the U12 device via Python.](https://github.com/Stickano/labJuice) This repository does not provide much value though, besides setting and receiving I/O values from the acquisition board. 
