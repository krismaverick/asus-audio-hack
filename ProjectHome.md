The headphone jacks on Asus G50/G60 laptops are prone to get stuck in a "plugged-in" state causing the onboard speakers to stop working.  This application provides a system tray icon that turns on/off the auto-detecting of the plugged in state of the headphones.  This allows the speakers to work.

## Detail ##

This tool provides a simple system tray icon to be able to control the sound card plug detection setting:

![http://asus-audio-hack.googlecode.com/svn/trunk/images/system-tray-icon.png](http://asus-audio-hack.googlecode.com/svn/trunk/images/system-tray-icon.png)

Right clicking the icon will present a menu to be able to choose between the auto-detecting mode and the Headphone + speaker mode.  The auto-detect mode is how the laptop normally works but since the headphones are stuck on, I called it "Headphone".  The other option "Speakers + Headphones" disables this auto-detection and will output through both channels.

![http://asus-audio-hack.googlecode.com/svn/trunk/images/rightclick.png](http://asus-audio-hack.googlecode.com/svn/trunk/images/rightclick.png)

When you select one, the registry setting will be changed and the device will be restarted.

![http://asus-audio-hack.googlecode.com/svn/trunk/images/popup-message.png](http://asus-audio-hack.googlecode.com/svn/trunk/images/popup-message.png)

You can also click "about" to see detail about the detected device in the registry and what setting it changes.

![http://asus-audio-hack.googlecode.com/svn/trunk/images/aboutbox.png](http://asus-audio-hack.googlecode.com/svn/trunk/images/aboutbox.png)


## Technical detail about what this does ##

I had found a post on the net about how you can change the following key, reboot the laptop, and the sound will work through the speakers: `SYSTEM\CurrentControlSet\Control\Class\{4D36E96C-E325-11CE-BFC1-08002BE10318}\0000\Settings\ForceDisableJD`.  You would set it to FF instead of 00.  Instead of rebooting, I would simply disable and re-enable my sound after changing this.  I had been doing this for awhile and got sick of all the steps involved so I made this tool to make it easier.

I have updated the tool to try to detect the card.  It simply searches for `ForceDisableJD` within the subkeys of the `"{4D36E96C-E325-11CE-BFC1-08002BE10318}"` section in the registry and then also searches the enum section for the device associated with it using the DeviceName it had determined from finding the ForceDisableJD.

I decided to make this public since I know there are a lot of people out there that have these models of laptops.

## Installation ##

Simply download the executable or grab the source and compile and add it to your Start Up menu.  .NET 3.5 is required.

## Notes ##

When you choose a new setting and it tries to restart the device, nothing can have the audio device open at that time or it fails to restart it.  It should simply pop up the error in the tooltip if this happens but the checked selection and actual state of the device will be out of sync.  Once the other application is closed, you can reselect the desired setting and it will apply.  If you select the same option twice, it will actually process that selection the second time too - setting the registry and restarting the device.

## Change Log ##
v1.0.1 - Change device search method to search `"{4D36E96C-E325-11CE-BFC1-08002BE10318}"` keys for ForceDisableJD instead of `"Realtek High Definition Audio"`.

v1.0 - First release.

