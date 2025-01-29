"use client";

import React, { useEffect, useState } from "react";
import { getConnection } from "../../services/signalrClient";
import CustomDialog from "../customDialog/customdialog";
import { Button, Typography, Box, Grid } from "@mui/material";
import { useRouter } from "next/navigation";
import "./queueDialog.css";
import { HubConnection } from '@microsoft/signalr';

interface QueueDialogProps {
  open: boolean;
  onClose: () => void;
  mode: string;
  timer: number;
}

const QueueDialog: React.FC<QueueDialogProps> = ({ open, onClose, mode, timer }) => {
  const [playersInQueue, setPlayersInQueue] = useState(1);
  const router = useRouter();

  useEffect(() => {
    if (!open) return;
  
    // Definiujemy callbacki
    const handlers = {
      WaitingForOpponent: (playerCount: number) => {
        setPlayersInQueue(playerCount);
      },
      GameReady: (gameId: string) => {
        onClose();
        router.push(`/play-online/${gameId}`);
      },
      Error: (message: string) => {
        alert(message);
      },
    };
  
    let hub: HubConnection | null = null;


    const initQueueConnection = async () => {
      try {
        hub = await getConnection(handlers);
        await hub.invoke("FindOpponent", hub.connectionId, mode, timer);
      } catch (error) {
        console.error("Failed to connect to SignalR Hub:", error);
        alert("Connection error. Please refresh the page.");
      }
    };
  
    initQueueConnection();
  
    return () => {
      // Cleanup SignalR connection if it exists
      (async () => {
        try {
          // if (hub && hub.state === "Connected") {
          //   await hub.stop();
          //   console.log("SignalR connection stopped in QueueDialog cleanup.");
          // }
        } catch (err) {
          console.error("Error stopping the SignalR connection in cleanup:", err);
        }
      })();
    };

  }, [open, mode, timer, onClose, router]);

  const leaveQueue = async () => {
    try {
      const hub = await getConnection();
      await hub.invoke("RemovePlayerFromQueue", hub.connectionId);
    } catch (error) {
      console.error("Failed to leave queue:", error);
    }
    onClose();
  };
  
  

  const dialogContent = (
    <Box textAlign="center" p={2}>
      <Grid container spacing={2} direction="column" alignItems="center" >
        <Grid item>
          <h1>Players in queue: {playersInQueue}</h1>
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
