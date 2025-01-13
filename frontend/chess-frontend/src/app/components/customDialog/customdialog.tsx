import React from "react";
import { Dialog, DialogContent, DialogActions, Typography } from "@mui/material";

const CustomDialog = ({ open, onClose, title, content, actions }) => {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      PaperProps={{
        style: {
          backgroundColor: "rgba(255, 255, 255, 0.1)",
          backdropFilter: "blur(10px)",
          borderRadius: "15px",
          border: "1px solid rgba(255, 255, 255, 0.2)",
          padding: "20px",
          boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)",
        },
      }}
    >
      <DialogContent>
        {title && (
          <Typography
            variant="h6"
            style={{
              color: "white",
              textAlign: "center",
              marginBottom: "15px",
            }}
          >
            {title}
          </Typography>
        )}
        {content}
      </DialogContent>
      <DialogActions style={{ justifyContent: "center" }}>{actions}</DialogActions>
    </Dialog>
  );
};

export default CustomDialog;
