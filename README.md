# AI Art Gallery

A personal project which uses OpenAI's API (DALL-E and GPT-4) to generate a unique art gallery experience based on a chosen theme. Each gallery features AI-generated artwork and narrative descriptions that come together to create a cohesive exhibition. The gameplay was created using Unity3D.

![Gallery Preview Placeholder](preview.gif)

## Demo Video (Click To Watch!)

[![Watch the Demo](https://github.com/user-attachments/assets/a60427d2-d77a-4460-acd1-4ac17d360daf)](https://youtu.be/QVHAl8b5wQo?si=-3h6S4Zyv45A5FHg)
![Still 2024-11-19 203321_1 3 5](https://github.com/user-attachments/assets/a60427d2-d77a-4460-acd1-4ac17d360daf)

![Still 2024-11-19 201834_1 5 2](https://github.com/user-attachments/assets/fd936ed2-1302-41fd-b6ee-f4e032656950)
![Still 2024-11-19 201818_1 3 3](https://github.com/user-attachments/assets/78b0a01a-0d91-4c99-b87f-3d68968d154c)
![Still 2024-11-19 201743_1 2 1](https://github.com/user-attachments/assets/5687c7d3-da70-41e4-8cae-8be46b7ef4c0)
![Still 2024-11-19 201837_1 5 3](https://github.com/user-attachments/assets/d9f64adc-710d-401e-a0bc-7c87e14bb443)



## Features

- **Theme-Based Generation**: Enter any theme and watch as AI creates a complete gallery exhibition
- **AI-Generated Content**:
  - Artwork created by DALL-E
  - Narrative descriptions written by GPT-4
  - Cohesive gallery storyline
- **Rate-Limited Generation**: Respects OpenAI API constraints with built-in timing controls

## Prerequisites

- Unity 2022.3 or higher
- OpenAI API key
- Git LFS (for handling large files)

## Dependencies

- DOTween
- Cinemachine
- UI Toolkit
- OpenAI Unity Package

## Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/AI-Art-Gallery.git
```

2. Open the project in Unity 2022.3 or higher

3. Create a `Config` asset in the project:
   - Right-click in the Project window
   - Select Create > ScriptableObjects > Config
   - Add your OpenAI API key to the Config asset

4. Install required packages through the Package Manager:
   - DOTween
   - Cinemachine
   - OpenAI Unity Package
  
  Note: In given circumstances I can provide the .exe file directly so you can skip this installation.

## Known Limitations

- Gallery generation takes 5-7 minutes due to OpenAI API rate limits
- Requires active internet connection
- API key needs appropriate credits/subscription
- Some themes may produce unexpected results
