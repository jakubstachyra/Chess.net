'use client';

import React, { useEffect, useState } from 'react';
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
} from '@mui/material';

function Rankings({ userId }) {
  const [rankings, setRankings] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
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
      } catch (error) {
        console.log(error);
        setError('Failed to fetch rankings. Please try again later.');
      } finally {
        setLoading(false);
      }
    };

    fetchRankings();
  }, [userId]);

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
    <Container component="main" maxWidth="md">
      <Box style={modalContentStyles}>
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
                <TableCell style={tableHeaderStyle} align="right">
                  Rating
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {rankings &&
                Object.entries(rankings).map(([category, rating]) => (
                  <TableRow key={category}>
                    <TableCell style={tableCellStyle}>{category}</TableCell>
                    <TableCell style={tableCellStyle} align="right">
                      {rating || 'N/A'}
                    </TableCell>
                  </TableRow>
                ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Box>
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

const tableHeaderStyle = {
  color: 'white',
  fontWeight: 'bold',
  textShadow: '-1px 1px 10px rgba(0, 0, 0, 0.75)',
};

const tableCellStyle = {
  color: 'white',
  textShadow: '-1px 1px 5px rgba(0, 0, 0, 0.5)',
};
