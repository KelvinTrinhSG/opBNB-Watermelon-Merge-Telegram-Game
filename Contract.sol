// SPDX-License-Identifier: MIT
pragma solidity ^0.8.17;

// Define the "WatermelonGame" contract
contract WatermelonGame {
    // Use a mapping to store the number of points each player owns.
    // `playerKeyPasses` is a mapping from the player's address to their points (uint256).
    mapping(address => uint256) private playerKeyPasses;

    // Events are used to record important changes on the blockchain:
    // Event `KeyPassAdded` is emitted when points are added to a player's account.
    event KeyPassAdded(address indexed player, uint256 newTotal);

    // Function to check the number of key passes a player currently owns.
    // Returns the key pass count.
    function getPlayerKeyPass(address player) external view returns (uint256) {
        return playerKeyPasses[player];
    }

    // Function to add key passes to a player's account.
    // - Requires the key pass amount to be greater than 0.
    // - Updates the total number of key passes the player owns.
    // - Emits the `KeyPassAdded` event with the new total key passes.
    function addKeyPass(address player, uint256 amount) external {
        require(amount > 0, "Amount must be greater than zero"); // Ensure the key pass amount is greater than 0.
        playerKeyPasses[player] += amount; // Add key passes to the player's current balance.
        emit KeyPassAdded(player, playerKeyPasses[player]); // Emit the event with the new total key passes.
    }
}
