import React from "react";
import { Dialog, DialogContent, DialogActions, Typography } from "@mui/material";

interface CustomDialogProps {
  open: boolean;
  onClose: () => void;
  title: string;
  content: React.ReactNode;
  actions?: React.ReactNode; // Opcjonalne, jeśli `actions` nie zawsze jest wymagane
}

const CustomDialog: React.FC<CustomDialogProps> = ({ open, onClose, title, content, actions }) => {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      PaperProps={{
        style: {
          backgroundColor: "rgba(255, 255, 255, 0.1)",
          boxShadow: "0 4px 15px rgba(0, 0, 0, 0.3)",
          backdropFilter: "blur(10px)",
          borderRadius: "15px",
        },
      }}
    >
      <Typography variant="h6" style={{ padding: "16px", color: "white", textAlign: "center" }}>
        {title}
      </Typography>
      <DialogContent>{content}</DialogContent>
      {actions && <DialogActions>{actions}</DialogActions>}
    </Dialog>
  );
};

export default CustomDialog;
