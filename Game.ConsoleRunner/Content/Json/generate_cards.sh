#!/bin/bash

# Генерация JSON файлов для всех 52 карт

RANKS=("Two" "Three" "Four" "Five" "Six" "Seven" "Eight" "Nine" "Ten" "Jack" "Queen" "King" "Ace")
SUITS=("Spades" "Hearts" "Diamonds" "Clubs")

for suit in "${SUITS[@]}"; do
    for rank in "${RANKS[@]}"; do
        id="${rank}_${suit}"
        filename="Cards/${id}.json"
        
        cat > "$filename" <<EOF
{
  "Id": "$id",
  "Rank": "$rank",
  "Suit": "$suit"
}
EOF
    done
done

echo "Generated 52 card JSON files"

