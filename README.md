# Birdie 

## A free Android app that makes all of Bernie's tweets visible to your Twitter followers

Birdie is a simple volunteer app that follows this basic workflow:

1\. You install the app on your phone.

2\. On first run, you'll be prompted to link your Twitter account.

3\. From here on, Birdie runs in the background, periodically checking Bernie's Twitter feeds and using your Twitter account to retweet everything he tweets.  

Those tweets are then visible to your Twitter followers, whether they follow Bernie or not.  **If even some of them like what they see and subsequently follow Bernie, this app will have served its purpose.**

If you restart your phone, Birdie automatically picks up where it left off.

4\. If at any point you want it to stop, just open Birdie and tap the big red "STOP" button.  Otherwise, the app is designed to continue running indefinitely without any further user interaction required.

**That's it.  Install, link to your Twitter account, and you're done.  Doesn't get much easier than that.**  =)

So if you have an Android phone and an *active* Twitter account (no dedicated bot accounts, please), you can be a YUGE help by installing the app and letting it run on your phone or tablet.  

# [Download Birdie on Google Play](https://play.google.com/store/apps/details?id=com.companyname.Birdie)

Birdie is 100% free (no in-app purchases) and contains no ads of any kind.  It is also open source and can be found on GitHub at:

**https://github.com/sirkris/Birdie**

Thanks for your help!

\#NotMeUs

## FAQ

**Q. Does Birdie comply with Twitter's rules?**

Yes, Birdie is 100%-compliant.  Automated retweets like this are explicitly permitted in the API rules.  Furthermore, the app distributes larger sets of tweets over time so as not to overload or spam the network.

While Twitter and other social media sites do make combating spam bots a priority, Birdie is not such a bot (and no, I'm not from Russia, either).  Birdie is designed to be used by real Twitter users with active accounts; people who would otherwise be inclined to retweet all of Bernie's tweets, anyway, if it were sufficiently convenient.  

This project does *not* welcome the use of dedicated bot accounts like those designed to manipulate Twitter's algorithms.  This is about making Bernie's tweets visible to your friends and family, exposing them to what he has to say on the issues despite the pervailing media blackout.  The hope is that at least some of them will, in turn, follow Bernie on Twitter, thereby increasing his Twitter reach.

This type of use-case is permitted, even on larger deployment scales.  The app does *not* auto-like tweets or do anything else that violates Twitter's API rules or ToS.

Worst-case scenario, if Twitter were to change their rules so this is no longer allowed, they would simply strip the app's permissions so it can no longer tweet.  The actual person using the app would be otherwise unaffected (so long as they're real and not just a dedicated spambot account, of course).  But again, I need to stress that Birdie is 100% compliant with Twitter's rules.  I was very careful about this.

**Q. Why not just make it have the user choose whether to retweet each tweet?**

There are already apps out there that can be used to do that.  Birdie is designed for exposing *all* of Bernie's recent tweets to your followers, in the hopes that at least some of them will then like what they see and follow him as a result.

There are already volunteers out there doing this manually throughout the day.  The whole point behind Birdie is to automate this process, freeing up those volunteers to work on other things, while also opening the door for people who otherwise wouldn't have time to volunteer to easily make a difference.

**Q. I installed the app, but if I leave it in the background for more than a few minutes, it stops running until I open it again.  What gives?**

Some phone manufacturers use a custom version of Android that prevents any apps not made by a "trusted" source like Google from running in the background for more than a few minutes.  They do this by suspending or terminating all of an app's scheduled background alarms and jobs.

This prevents Birdie from periodically executing its background tasks as intended.  Unfortunately, on some devices (like my fucking TracFone), there is simply no way to turn this behavior off and no way to get around it as an app developer.

So if this happens to you, here's a simple and only mildly inconvenient workaround:  Two or three times a day, open Birdie on your phone, wait for it to load, then hit your phone's home button to go back.  That will force the app back into the foreground and the operating system will stop blocking it from working.

**Q. When can we expect an iPhone version of Birdie?**

Short answer:  I don't know.

Long answer:  [Here](https://www.reddit.com/r/StillSandersForPres/comments/es93dh/ive_built_a_new_android_app_that_quietly_runs_in/ff8y4yx/?context=3).
