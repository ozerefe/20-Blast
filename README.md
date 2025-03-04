20 BLAST!
Tile-Matching Blast Game is a colorful puzzle game developed in Unity, where players interact with a grid of blocks in different colors. The goal is to tap on groups of two or more matching blocks to clear them from the board, earning points and allowing new blocks to fall into place.

Key Features:

Colorful blocks with dynamic sprite changes based on group size (A, B, C thresholds).
Groups smaller than 2 blocks cannot be cleared, encouraging strategic play.
Each tap consumes a move whether it triggers a blast or not, adding challenge and planning to the gameplay.
The game ends after 20 moves, displaying the final score and allowing a restart.
The highest score achieved is saved and shown to the player.
If the board reaches a state with no valid matches, a shuffle system automatically rearranges the blocks to keep gameplay flowing.
Performance Considerations:

Efficient use of sprite resources to minimize memory footprint.
Simple but effective logic for group detection and sprite updates to reduce CPU overhead.
Minimal unnecessary UI updates and smart refresh logic to optimize GPU usage.
This project is part of a case study focused on creating an optimized and playable collapse/blast puzzle game with clear performance goals.
