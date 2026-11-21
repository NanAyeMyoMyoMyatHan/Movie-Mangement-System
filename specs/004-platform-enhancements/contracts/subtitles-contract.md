# Contract: Subtitle & Audio Track Protocol (.vtt Support)

**Feature Branch**: `004-platform-enhancements`  
**Date**: 2026-11-21  

This contract defines the format, endpoints, and markup specification for Myanmar and multi-language subtitles using WebVTT format.

---

## 1. WebVTT File Format Specification

All subtitle files MUST adhere to the WebVTT standard (`RFC 8216 / W3C WebVTT`) with UTF-8 encoding.

### Sample WebVTT File (`/subtitles/alchemy_ep1_my.vtt`):
```vtt
WEBVTT - NangWorld Myanmar Subtitle Track

1
00:00:05.120 --> 00:00:08.500
မင်္ဂလာပါ NangWorld မှ ကြိုဆိုပါတယ်။

2
00:00:09.000 --> 00:00:13.200
ဒယ်ဟိုနိုင်ငံတော်ရဲ့ မှော်စွမ်းအင်ရှင်များအကြောင်း...
```

---

## 2. Server Static File Serving & MIME Type

In `Program.cs` (`DotNetProject.Balzor`):
```csharp
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".vtt"] = "text/vtt; charset=utf-8";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});
```

---

## 3. HTML5 Player `<track>` Markup Specification

In `DramaDetail.razor`:

```html
<video id="mainVideoPlayer" class="drama-video" src="@GetPlaybackUrl()" controls preload="metadata">
    @if (!string.IsNullOrWhiteSpace(ActiveSubtitleUrl))
    {
        <track kind="subtitles" 
               label="Myanmar (မြန်မာ)" 
               srclang="my" 
               src="@ActiveSubtitleUrl" 
               default />
    }
    @if (!string.IsNullOrWhiteSpace(ActiveEnglishSubtitleUrl))
    {
        <track kind="subtitles" 
               label="English" 
               srclang="en" 
               src="@ActiveEnglishSubtitleUrl" />
    }
    Your browser does not support the video tag.
</video>
```

---

## 4. Subtitle Track Model Contract

```csharp
public class SubtitleTrackDto
{
    public string Language { get; set; } = "my"; // "my", "en", "th"
    public string Label { get; set; } = "Myanmar (မြန်မာ)";
    public string Url { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = true;
}
```
