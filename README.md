## What is Messenger Chat Bot?
After looking for a long time for something like this, I really wasn't able to find a good example of what I was looking for on the internet. 
Messenger Chat Bot is a project that allows you to directly read/send messages and send files as well.
It's using a WebBrowser component from Visual Studio to read messages from a chat. 

You'll most likely need to have another Facebook account dedicated for the use of the bot or use one for everything. The decision is all yours.


## Setup
 
1. Once you make the new facebook account or decide to use yours, you need to go and change your Facebook language by going [here](https://www.facebook.com/settings?tab=language&view) and changing your "Facebook language" to "English (UK)"

1. Open "Form1.cs" and find the region called "Variables To Change", inside make sure to change the ChatID to the person/chat group ID. 
    1. You can find a person's facebook ID by his username, by going to [lookup-id.com](https://lookup-id.com/)
    1. You can find a group's chat ID by going in the [Facebook's web messenger app](https://www.facebook.com/messages/), clicking on the group and looking at the URL, it will be at the end.

1. Then make sure to set the login details, from the "Login Details" region to the ones you have on your bot. 
I can assure you, 100%, your information is not being logged somewhere, you can feel free to look over the whole code to make sure.

1. Once you open the program two windows will pop-up, one of them is the browser, the other one is the control panel. Simply press Start and feel free to minimize everything. I suggest you mute the application because you'll be able to hear when you receive a message.



## A few notes

- If the bot decides to crash or act laggy, just click the Restart button and it's going to work just fine. It usually happens if the chat is really active, a lot of people are writing and stuff, and the machine you're running the bot on can't exactly handle it. Apparently it happens due to memory leaks from the WebBrowser component.

- Feel free to modify the bot to your own needs.

- Please, if you can, credit me for the work! It will be much appreciated.



## Donations

If you're willing to help me out, thank me or just give me a small donation you can! It's not mandatory, but I'd appreciate it, for me it's a way to see that I am doing somewhat good.

You can donate me from [here](https://www.paypal.me/DimitriSSS)