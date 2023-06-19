import React, {useMemo, useEffect, useState} from "react";

// MRT Imports
// import MaterialReactTable from 'material-react-table'; // default import deprecated
import {MaterialReactTable} from "material-react-table";

// Material UI Imports
import {Box, Button, ListItemIcon, MenuItem, Typography} from "@mui/material";

// Icons Imports
import {Delete, Edit} from "@mui/icons-material";

import Chip from "@mui/material/Chip";

const Records = () => {
    const [data, setData] = useState([]);

    useEffect(() => {
        const fetchData = async () => {
            const response = await fetch("records");
            const recordsData = await response.json();
            setData(recordsData);
        };

        fetchData();
    }, []);

    const columns = useMemo(
        () => [
            {
                accessorFn: (row) => `${row.label}`,
                id: "name",
                header: "Name",
                size: 250,
            },
            {
                accessorFn: (row) => row.isActive ? 'true' : 'false',
                header: "Status",
                size: 150,
                filterVariant: 'checkbox',
                Cell: ({cell}) => (
                    <Box
                        component="span"
                        sx={(theme) => ({
                            backgroundColor:
                                cell.getValue() === 'true'
                                    ? theme.palette.success.dark
                                    : theme.palette.error.dark,
                            borderRadius: "0.25rem",
                            color: "#fff",
                            maxWidth: "9ch",
                            p: "0.25rem",
                        })}
                    >
                        {cell.getValue() === 'true' ? "Active" : "Inactive"}
                    </Box>
                ),
            },
            {
                accessorKey: "created",
                id: "creationDate",
                header: "Created",
                size: 250,
                Cell: ({cell}) => new Date(cell.getValue()).toLocaleDateString(),
            },
        ],
        []
    );

    return (
        <MaterialReactTable
            columns={columns}
            data={data}
            enableColumnFilterModes
            enableGrouping
            enableRowActions
            enableRowSelection
            initialState={{showColumnFilters: true}}
            positionToolbarAlertBanner="bottom"
            renderDetailPanel={({row}) => (
                <Box>
                    <Typography variant="body1">
                        <strong>ID:</strong> {row.original.id}
                    </Typography>
                    <Typography variant="body1">
                        <strong>Exact creation time:</strong>{" "}
                        {new Date(row.original.created).toLocaleString()}
                    </Typography>
                    <Typography variant="body1">
                        <strong>Tags: </strong>
                        {row.original.tags.map((tag) => (
                            <Chip
                                key={tag.id}
                                label={tag.name}
                                variant="outlined"
                                sx={{
                                    margin: "0.2rem",
                                    color: "purple",
                                    borderColor: "purple",
                                    fontWeight: "bold",
                                }}
                            />
                        ))}
                    </Typography>
                    <Typography variant="body1">
                        <strong>Crawl Info:</strong>
                        <ul>
                            <li>
                                <strong>Entry URL: </strong>
                                <a
                                    href={row.original.crawlInfo.entryUrl}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                >
                                    {row.original.crawlInfo.entryUrl}
                                </a>
                            </li>
                            <li>
                                <strong>Regex Pattern:</strong>{" "}
                                {row.original.crawlInfo.regexPattern}
                            </li>
                            <li>
                                <strong>Periodicity:</strong>{" "}
                                {row.original.crawlInfo.periodicity}
                            </li>
                        </ul>
                    </Typography>
                </Box>
            )}
            renderRowActionMenuItems={({closeMenu}) => [
                <MenuItem
                    key={0}
                    onClick={() => {
                        // Delete logic...
                        closeMenu();
                    }}
                    sx={{m: 0}}
                >
                    <ListItemIcon>
                        <Delete/>
                    </ListItemIcon>
                    Delete
                </MenuItem>,
                <MenuItem
                    key={1}
                    onClick={() => {
                        // Edit logic...
                        closeMenu();
                    }}
                    sx={{m: 0}}
                >
                    <ListItemIcon>
                        <Edit/>
                    </ListItemIcon>
                    Edit
                </MenuItem>,
            ]}
            renderTopToolbarCustomActions={({table}) => {
                const handleDeactivate = () => {
                    table.getSelectedRowModel().flatRows.map((row) => {
                        alert("deactivating " + row.getValue("name"));
                        return null;
                    });
                };

                const handleActivate = () => {
                    table.getSelectedRowModel().flatRows.map((row) => {
                        alert("activating " + row.getValue("name"));
                        return null;
                    });
                };

                return (
                    <div style={{display: "flex", gap: "0.5rem"}}>
                        <Button
                            color="error"
                            disabled={!table.getIsSomeRowsSelected()}
                            onClick={handleDeactivate}
                            variant="contained"
                        >
                            Deactivate
                        </Button>
                        <Button
                            color="success"
                            disabled={!table.getIsSomeRowsSelected()}
                            onClick={handleActivate}
                            variant="contained"
                        >
                            Activate
                        </Button>
                    </div>
                );
            }}
        />
    );
};

export default Records;
