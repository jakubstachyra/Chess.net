import React, { useState } from "react";
import { useSelector } from "react-redux";
import ListDisplay from "../listDisplay/listDisplay";
import { Button, Box, IconButton, Typography } from "@mui/material";
//import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import CloseIcon from "@mui/icons-material/Close";
import CustomDialog from "../customDialog/customdialog";

    const FriendsList = ({ friends, onRemoveFriend }) => {
    const [openDialog, setOpenDialog] = useState(false);
    const [selectedFriend, setSelectedFriend] = useState(null);
    const [removalLoading, setRemovalLoading] = useState(false); // Dodany stan dla ładowania

    // Pobierz userId z Redux
    const reduxUser = useSelector((state) => state.user);
    const user = reduxUser.user;

    const handleOpenDialog = (friend) => {
        setSelectedFriend(friend);
        setOpenDialog(true);
    };

    const handleCloseDialog = () => {
        setOpenDialog(false);
        setSelectedFriend(null);
    };

    const handleConfirmRemove = async () => {
        if (selectedFriend) {
        try {
            setRemovalLoading(true); // Włącz ładowanie
            const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

            const response = await fetch(`${API_BASE_URL}/friends/${user.userID}`, {
            method: "DELETE",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(selectedFriend.id), // Przesyłamy tylko nazwę znajomego jako string
            });
            
            if (!response.ok) {
            throw new Error(`HTTP Error: ${response.status}`);
            }

            onRemoveFriend(selectedFriend.id); // Aktualizuj listę znajomych na frontendzie
            handleCloseDialog(); // Zamknij dialog
        } catch (error) {
            console.error("Error removing friend:", error);
            alert("Failed to remove friend. Please try again.");
        } finally {
            setRemovalLoading(false); // Wyłącz ładowanie
        }
        }
    };

    const renderFriendRow = (friend) => (
        <Box style={friendRowStyles}>
        <span style={friendStyles}>{friend.name}</span>
        <Box style={actionsContainerStyles}>
            {/* <Button
            variant="contained"
            color="success"
            size="small"
            startIcon={<PlayArrowIcon />}
            sx={{ marginRight: "5px", padding: "4px 10px" }}
            onClick={() => console.log(`Play with ${friend.name}`)}
            >
            Play
            </Button> */}
            <IconButton
            color="error"
            size="small"
            onClick={() => handleOpenDialog(friend)}
            >
            <CloseIcon />
            </IconButton>
        </Box>
        </Box>
    );

    return (
        <>
        <ListDisplay data={friends} renderRow={renderFriendRow} />
        <CustomDialog
            open={openDialog}
            onClose={handleCloseDialog}
            title="Confirm Removal"
            content={
            <Typography
                variant="body1"
                style={{
                color: "white",
                textAlign: "center",
                }}
            >
                Are you sure you want to remove <b>{selectedFriend?.name}</b> from your friends list?
            </Typography>
            }
            actions={
            <>
                <Button
                onClick={handleCloseDialog}
                color="primary"
                variant="outlined"
                sx={{ color: "white", borderColor: "white" }}
                disabled={removalLoading} // Zablokuj przycisk podczas ładowania
                >
                Cancel
                </Button>
                <Button
                onClick={handleConfirmRemove}
                color="secondary"
                variant="contained"
                sx={{
                    backgroundColor: "#d32f2f",
                    color: "white",
                    marginLeft: "10px",
                }}
                disabled={removalLoading} // Zablokuj przycisk podczas ładowania
                >
                {removalLoading ? "Removing..." : "Remove"}
                </Button>
            </>
            }
        />
        </>
    );
    };

// Style dla wiersza znajomego
const friendRowStyles = {
display: "flex",
justifyContent: "space-between",
alignItems: "center",
width: "100%",
padding: "8px",
borderRadius: "5px",
marginBottom: "5px",
};

// Style dla tekstu znajomego
const friendStyles = {
color: "white",
textAlign: "left",
flex: 1,
fontFamily: "monospace",
fontSize: "0.9rem",
};

// Styl dla kontenera akcji
const actionsContainerStyles = {
display: "flex",
alignItems: "center",
};

export default FriendsList;
