"use client";

import React, { useEffect, useState } from "react";
import { getConnection } from "../../services/signalrClient";
import CustomDialog from "../customDialog/customdialog";
import { Button, Typography, Box, Grid } from "@mui/material";
import { useRouter } from "next/navigation";
import "./queueDialog.css";

interface QueueDialogProps {
  open: boolean;
  onClose: () => void;
  mode: string;
  timer: number;
}

const QueueDialog: React.FC<QueueDialogProps> = ({ open, onClose, mode, timer }) => {
  const [playersInQueue, setPlayersInQueue] = useState(0);
  const router = useRouter();

  useEffect(() => {
    if (!open) return;

    // Definiujemy callbacki/handlery, które będą obsługiwane podczas czekania w kolejce
    const handlers = {
      WaitingForOpponent: (playerCount: number) => {
        setPlayersInQueue(playerCount);
      },
      GameReady: (gameId: string) => {
        // Zamykamy dialog, przechodzimy do gry
        onClose();
        router.push(`/play-online/${gameId}`);
      },
      Error: (message: string) => {
        alert(message);
      },
    };

    let isMounted = true;
    const initQueueConnection = async () => {
      try {
        // Pobieramy lub tworzymy globalne połączenie
        const hub = await getConnection(handlers);
        // Wywołujemy metodę huba "FindOpponent", by dołączyć do kolejki
        await hub.invoke("FindOpponent", hub.connectionId, mode, timer);
      } catch (error) {
        console.error("Failed to connect to SignalR Hub:", error);
        alert("Connection error. Please refresh the page.");
      }
    };

    initQueueConnection();

    return () => {
      // UWAGA: specjalnie *nie* zatrzymujemy połączenia (hub.stop()),
      // bo chcemy je zachować, aby serwer nie wykrył rozłączenia.
      isMounted = false;
    };
  }, [open, mode, timer, onClose, router]);

  const leaveQueue = async () => {
    try {
      // W każdej chwili możemy pobrać istniejące połączenie
      const hub = await getConnection();
      await hub.invoke("RemovePlayerFromQueue", hub.connectionId);
    } catch (error) {
      console.error("Failed to leave queue:", error);
    }
    onClose();
  };

  const dialogContent = (
    <Box textAlign="center" p={2}>
      <Grid container spacing={2} direction="column" alignItems="center">
        <Grid item>
          <Box mt={2} mb={2} display="flex" justifyContent="center">
            <div className="spinner"></div>
          </Box>
        </Grid>
        <Grid item>
          <Typography variant="h6" style={{ color: "white" }}>
            {timer / 60} minutes
          </Typography>
        </Grid>
        <Grid item>
          <Button
            variant="contained"
            sx={{
              backgroundColor: "#FF0000",
              marginLeft: "10px",
              width: "100%",
            }}
            onClick={leaveQueue}
          >
            Leave queue
          </Button>
        </Grid>
      </Grid>
    </Box>
  );

  return (
    <CustomDialog
      open={open}
      onClose={onClose}
      title="Waiting in queue"
      content={dialogContent}
      actions={null}
    />
  );
};

export default QueueDialog;
