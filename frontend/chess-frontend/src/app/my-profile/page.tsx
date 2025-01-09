'use client';

import React, { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import FriendsList from '../components/friendsList/friendsList';
import {
  Container,
  Typography,
  CircularProgress,
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Alert,
  Button,
  TextField,
} from '@mui/material';
import BackgroundUI from 'app/components/backgroundUI/pages';
import CustomDialog from '../components/customDialog/customDialog';

function Rankings() {
  const [rankings, setRankings] = useState(null);
  const [friends, setFriends] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [inviteOpen, setInviteOpen] = useState(false);
  const [inviteInput, setInviteInput] = useState('');
  const [confirmButtonColor, setConfirmButtonColor] = useState('primary');
  const [inviteError, setInviteError] = useState(false); // Stan błędu dla zaproszenia
  const userId = useSelector((state) => state.user.user?.id);

  const fetchFriends = async () => {
    try {
      const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
      const response = await fetch(`${API_BASE_URL}/friends/${userId}`);
      if (!response.ok) {
        throw new Error(`HTTP Error: ${response.status}`);
      }
      const data = await response.json();
      setFriends(data);
    } catch (err) {
      setError('Failed to fetch friends. Please try again later.');
    }
  };

  useEffect(() => {
    const fetchRankings = async () => {
      try {
        const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
        const response = await fetch(`${API_BASE_URL}/getUserRankingsByUserID/${userId}`);
        if (!response.ok) {
          throw new Error(`HTTP Error: ${response.status}`);
        }
        const data = await response.json();
        setRankings(data);
      } catch (err) {
        setError('Failed to fetch rankings. Please try again later.');
      } finally {
        setLoading(false);
      }
    };

    if (userId) {
      fetchRankings();
      fetchFriends();
    }
  }, [userId]);

  const handleInviteOpen = () => {
    setInviteOpen(true);
    setInviteError(false); // Reset błędu
  };

  const handleInviteClose = () => {
    setInviteOpen(false);
    setInviteInput('');
    setConfirmButtonColor('primary'); // Reset koloru przycisku
    setInviteError(false); // Reset błędu
  };

  const handleSendInvite = async () => {
    try {
      if (!inviteInput || inviteInput.trim() === '') {
        alert('Friend Name is required.');
        return;
      }

      const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
      const response = await fetch(`${API_BASE_URL}/friends/${userId}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(inviteInput.trim()), // Przesyłanie czystego stringa
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`HTTP Error: ${response.status}, Details: ${errorText}`);
      }

      setConfirmButtonColor('success');
      await fetchFriends(); // Odświeżenie listy znajomych
      setTimeout(() => handleInviteClose(), 1000);
    } catch (err) {
      setInviteError(true); // Ustawienie błędu
    }
  };

  if (loading) {
    return (
      <Box style={modalContentStyles}>
        <CircularProgress />
        <Typography variant="h6" style={{ color: 'white', marginTop: '10px' }}>
          Loading Rankings...
        </Typography>
      </Box>
    );
  }

  if (error) {
    return (
      <Box style={modalContentStyles}>
        <Alert severity="error">{error}</Alert>
      </Box>
    );
  }

  return (
    <Container component="main" maxWidth="lg">
      <Box style={rankingsContainerStyle}>
        {/* Rankings Table */}
        <Box style={rankingsTableStyle}>
          <BackgroundUI>
            <Typography
              variant="h5"
              style={{
                color: 'white',
                textShadow: '-1px 1px 10px rgba(0, 0, 0, 0.75)',
                marginBottom: '20px',
              }}
            >
              Rankings
            </Typography>
            <TableContainer component={Paper} style={{ backgroundColor: 'rgba(255, 255, 255, 0.1)' }}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell style={tableHeaderStyle}>Category</TableCell>
                    <TableCell style={tableHeaderStyle}>Description</TableCell>
                    <TableCell style={tableHeaderStyle} align="right">
                      Rating
                    </TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {rankings &&
                    rankings.map((ranking) => (
                      <TableRow key={ranking.Ranking}>
                        <TableCell style={tableCellStyle}>{ranking.Ranking}</TableCell>
                        <TableCell style={tableCellStyle}>{ranking.RankingInfo}</TableCell>
                        <TableCell style={tableCellStyle} align="right">
                          {ranking.Points || 'N/A'}
                        </TableCell>
                      </TableRow>
                    ))}
                </TableBody>
              </Table>
            </TableContainer>
          </BackgroundUI>
        </Box>

        {/* Friends List */}
        <Box style={friendsListStyle}>
          <Typography
            variant="h6"
            style={{
              color: 'white',
              textShadow: '-1px 1px 10px rgba(0, 0, 0, 0.75)',
              marginBottom: '20px',
            }}
          >
            Friends
          </Typography>
          <FriendsList friends={friends} onRemoveFriend={fetchFriends} />
          <Button
            variant="contained"
            color="primary"
            onClick={handleInviteOpen}
            style={{
              margin: '20px auto 0', // Wyśrodkowanie przycisku
              display: 'block',
            }}
          >
            Invite Friend
          </Button>
        </Box>
      </Box>

      {/* Invite Friend Dialog */}
      <CustomDialog
        open={inviteOpen}
        onClose={handleInviteClose}
        title="Invite Friend"
        content={
          <>
            <TextField
              autoFocus
              margin="dense"
              label="Friend Name"
              type="text"
              fullWidth
              value={inviteInput}
              error={inviteError} // Ustawienie błędu
              helperText={inviteError ? 'User does not exist!' : ''}
              onChange={(e) => {
                setInviteInput(e.target.value);
                setInviteError(false); 
              }}
              sx={{
                '& .MuiInputBase-root': { color: 'white' },
                '& .MuiInputLabel-root': { color: 'white' },
                '& .MuiOutlinedInput-root.Mui-error': { borderColor: 'red' }, 
              }}
            />
          </>
        }
        actions={
          <Button
            onClick={handleSendInvite}
            color={confirmButtonColor}
            variant="contained"
            sx={{
              margin: '0 auto',
              display: 'block',
              backgroundColor: confirmButtonColor === 'success' ? 'green' : 'lightgreen',
              color: confirmButtonColor === 'success' ? 'white' : 'grey',
            }}
          >
            {confirmButtonColor === 'success' ? 'Sent!' : 'Send Invite'}
          </Button>
        }
      />
    </Container>
  );
}

export default Rankings;

const modalContentStyles = {
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  padding: '20px',
  backgroundColor: 'rgba(255, 255, 255, 0.1)',
  borderRadius: '15px',
  boxShadow: '0 4px 30px rgba(0, 0, 0, 0.5)',
  backdropFilter: 'blur(10px)',
  border: '1px solid rgba(255, 255, 255, 0.2)',
};

const rankingsContainerStyle = {
  display: 'flex',
  justifyContent: 'space-between',
  gap: '20px',
};

const rankingsTableStyle = {
  flex: 2,
};

const friendsListStyle = {
  flex: 1,
  backgroundColor: 'rgba(255, 255, 255, 0.1)',
  borderRadius: '15px',
  padding: '20px',
  boxShadow: '0 4px 30px rgba(0, 0, 0, 0.5)',
  backdropFilter: 'blur(10px)',
  border: '1px solid rgba(255, 255, 255, 0.2)',
};

const tableHeaderStyle = {
  color: 'white',
  fontWeight: 'bold',
  textShadow: '-1px 1px 10px rgba(0, 0, 0, 0.75)',
};

const tableCellStyle = {
  color: 'white',
  textShadow: '-1px 1px 5px rgba(0, 0, 0, 0.5)',
};
