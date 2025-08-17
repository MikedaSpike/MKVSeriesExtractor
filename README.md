# MKV Serie Extractor

![GitHub release](https://img.shields.io/github/v/release/MikeDaSpike/MKVSeriesExtractor)
![License](https://img.shields.io/github/license/MikeDaSpike/MKVSeriesExtractor)

## Overview

**MakeMKV Serie Extractor** is a tool designed to help you digitize and organize your DVD collection into clean, structured episode files. While I personally use it with [Jellyfin](https://jellyfin.org/), it works just as well with any media server or library system that benefits from properly named video files.

---

## Table of Contents

- [Overview](#overview)
- [Why I Made This](#why-i-made-this)
- [Features](#features)
- [Requirements](#requirements)
- [How It Works](#how-it-works)
- [Getting Started](#getting-started)
- [Installation](#installation)
- [Setup Steps for Extracting a Full TV Show](#setup-steps-for-extracting-a-full-tv-show)
- [Automatic Title Generation](#automatic-title-generation)
- [Contributing](#contributing)
- [License](#license)

---

## Why I Made This

I created this tool to solve a personal challenge: converting my extensive DVD collection into a format that works well with Jellyfin. Using MakeMKV helped extract the content, but the results were messy—lots of unwanted extras, poor naming, and no clear episode structure.

My biggest test was *E.R. (Emergency Room)*, which spans 15 seasons and includes tons of extras like “Play All” tracks. This tool handled it well (though it takes some time) and produced properly named files like:

```
/ER (1994–2009)/Season 01/
├── ER (1994–2009) - S01E01 - 24 Hours.mkv
├── ER (1994–2009) - S01E02 - Day One.mkv
├── ER (1994–2009) - S01E03 - Going Home.mkv
...
```

Based on a title list like:

```
S01E01: 24 Hours  
S01E02: Day One  
S01E03: Going Home  
S01E04: Hit and Run  
S01E05: Into That Good Night  
...
```

This tool helped me clean up the clutter and enjoy my collection in Jellyfin the way it was meant to be.

---

## Features

- ✅ Automatically renames extracted episodes using a provided title list  
- ✅ Filters out unwanted extras and “Play All” tracks  
- ✅ Organizes episodes into season folders  
- ✅ Produces Jellyfin-compatible filenames  
- ✅ Works with MakeMKV output (ISO or `VIDEO_TS`)

---

## Requirements

- Windows OS  
- Licensed version of [MakeMKV](https://www.makemkv.com/)  
- .NET Framework (if applicable)

---

## How It Works

1. Use MakeMKV Serie Extractor to extract content from your DVD  
2. Provide a title list in the format:

   ```
   S01E01: Episode Title  
   S01E02: Episode Title  
   ...
   ```

3. Run **MKVSeriesExtractor** on the extracted files  
4. The tool will:
   - Match titles to episodes  
   - Rename files accordingly  
   - Organize them into season folders

---

## Getting Started

Just download the latest release from the [Releases](https://github.com/MikeDaSpike/MKVSeriesExtractor/releases) page.

---

## Installation

- Extract the contents to any location.  
- Run `MKVSeriesExtractor.exe`.

---

## Setup Steps for Extracting a Full TV Show

1. **Fill in the series name**  
   You can include the year (e.g., `ER (1994–2009)`) or leave it out.

2. **Select the input directory**  
   This is the location where your ISO files or `VIDEO_TS` folders are stored.  
   You can also point directly to a `VIDEO_TS` folder on a DVD disc.

3. **Provide a title list (optional but recommended)**  
   The list must exactly match the episode titles on the DVDs.  
   If the titles don’t match, the tool may assign incorrect names—MakeMKV Serie Extractor does not search for correct matches.

4. **Set the path to `makemkvcon.exe`**  
   If the tool doesn’t automatically find it, browse manually.  
   **Important**: You must use a licensed version of MakeMKV.

5. **Set episode duration**  
   You can manually enter the average duration of an episode, or click the **Duration** button.  
   Make sure a reference DVD folder is selected before using this feature.

6. **Preview and select tracks**  
   The preview will show combined tracks.  
   Click on a track that represents a TV episode, then click **Durations** to apply it.

7. **Start the extraction**  
   Click **Start** to begin processing.  
   If you’ve extracted multiple DVDs, sit back and relax while the tool works through them.

---

## Automatic Title Generation

If you don’t want to use a title list, enable the **"Automatically generate titles"** checkbox.  
This will extract episodes from all DVDs and name them using the input DVD name, with episode numbers assigned per disc (e.g., `DiscName - E01`, `DiscName - E02`, etc.).

---

## Contributing

This tool was built to organize my personal DVD collection, and it works well for that.  
However, it may not handle every DVD structure perfectly.

If you run into issues with your own collection:

- Feel free to **fork the repository**  
- Make any changes needed  
- Submit a **pull request (PR)** to help improve the tool for others

Your contributions are welcome!

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
```

