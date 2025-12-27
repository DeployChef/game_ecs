#!/bin/bash
ranks=("Two" "Three" "Four" "Five" "Six" "Seven" "Eight" "Nine" "Ten" "Jack" "Queen" "King" "Ace")
suits=("Spades" "Hearts" "Diamonds" "Clubs")

for suit in "${suits[@]}"; do
  for rank in "${ranks[@]}"; do
    filename="${rank}_${suit}.json"
    cat > "$filename" << JSON
{
  "Id": "${rank}_${suit}",
  "Rank": "${rank}",
  "Suit": "${suit}"
}
JSON
  done
done
