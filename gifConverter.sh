#!/bin/bash

# Directory containing GIF files (change "." to your target directory if needed)
input_dir="."

# Loop through all GIF files in the directory
for gif_file in "$input_dir"/*.gif; do
  # Skip if no GIF files are found
  [ -e "$gif_file" ] || continue

  # Generate the output MP4 filename
  mp4_file="${gif_file%.gif}.mp4"

  # Convert the GIF to MP4 using ffmpeg
  ffmpeg -y -i "$gif_file" -vf "scale=trunc(iw/2)*2:trunc(ih/2)*2" \
    -pix_fmt yuv420p -vcodec libx264 -preset slow -crf 23 -profile:v baseline -level 3.0 \
    -movflags +faststart \
    -colorspace bt709 -color_primaries bt709 -color_trc bt709 \
    -video_track_timescale 30 "$mp4_file"

  echo "Converted $gif_file to $mp4_file"
done
