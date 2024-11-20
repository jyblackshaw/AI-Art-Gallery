# AI Art Gallery

A personal project which uses OpenAI's API (DALL-E and GPT-4) to generate a unique art gallery experience based on a chosen theme. Each gallery features AI-generated artwork and narrative descriptions that come together to create a cohesive exhibition. The gameplay was created using Unity3D.

![Gallery Preview Placeholder](preview.gif)

## Demo Video (Click To Watch!)

[![Watch the Demo](![Still 2024-11-19 203321_1 3 5](https://github.com/user-attachments/assets/b2c25198-7056-4aee-84bd-4b1b1ec92912))]([https://youtu.be/E7X-MG05KRQ](https://www.youtube.com/watch?v=QVHAl8b5wQo&t=2s))

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
