import React, { useMemo, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
    Box,
    Button,
    ListItemIcon,
    MenuItem,
    Typography,
} from "@mui/material";
import { Delete, Edit } from "@mui/icons-material";
import Chip from "@mui/material/Chip";
import {
    MaterialReactTable,
    MRT_ColumnDef,
} from "material-react-table";

interface WebsiteRecord {
    id: number;
    label: string;
    isActive: boolean;
    tags: Tag[];
    created: string;
    crawlInfo: CrawlInfo;
}

interface CrawlInfo {
    jobId?: number;
    entryUrl: string;
    regexPattern: string;
    periodicity: string;
    websiteRecordId: number;
}

interface Tag {
    id: number;
    name: string;
}

const Records: React.FC<{ showEditModalWindow: Function, registerDataUpdateHandler: Function }> = ({
    showEditModalWindow, registerDataUpdateHandler,
}) => {
    const [data, setData] = useState<WebsiteRecord[]>([]);
    const navigate = useNavigate();

    const addNewRecord = async (id: Number) => {
        let response = await fetch(`record/${id}`);
        let record = await response.json() as WebsiteRecord;

        console.log(record);

        let found = false;
        data.forEach((row) => {
            if (row.id === id) {
                row = record;
                found = true;
            }
        });

        if (!found) {
            setData([...data, record]);
            console.log("New record added with id: " + id);
        } else {
            setData([...data]);
            console.log("Record updated with id: " + id);
        }
    };

    useEffect(() => {
        const fetchData = async () => {
            const response = await fetch("records");
            const recordsData = await response.json();
            setData(recordsData);
            console.log(recordsData);
        };

        fetchData();
        registerDataUpdateHandler(addNewRecord);
    }, []);

    const columns: MRT_ColumnDef<WebsiteRecord>[] = useMemo(
        () => [
            {
                accessorFn: (row) => `${row.label}`,
                id: "name",
                header: "Name",
                size: 250,
            },
            {
                accessorFn: (row) => (row.isActive ? "true" : "false"),
                header: "Status",
                size: 150,
                filterVariant: "checkbox",
                Cell: ({ cell }) => (
                    <Box
                        component="span"
                        sx={(theme) => ({
                            backgroundColor:
                                cell.getValue() === "true"
                                    ? theme.palette.success.dark
                                    : theme.palette.error.dark,
                            borderRadius: "0.25rem",
                            color: "#fff",
                            maxWidth: "9ch",
                            p: "0.25rem",
                        })}
                    >
                        {cell.getValue() === "true" ? "Active" : "Inactive"}
                    </Box>
                ),
            },
            {
                accessorKey: "created",
                id: "creationDate",
                header: "Created",
                size: 100,
                filterVariant: "text",
                filterFn: (row, id, filterValue) => {
                    const date = new Date(row.getValue<string>(id));
                    const dateStr = `${date.getDate()}. ${date.getMonth()}. ${date.getFullYear()}`;
                    return dateStr.includes(filterValue);
                },
                Cell: ({ cell }) =>
                    new Date(cell.getValue<string>()).toLocaleDateString(),
            },
            {
                accessorKey: "tags",
                id: "tags",
                header: "Tags",
                size: 300,
                filterVariant: "text",
                filterFn: (row, id, filterValue) => {
                    const tags: Tag[] = row.getValue<Tag[]>(id);
                    console.log(tags);
                    return tags.some((tag) =>
                        tag.name.toLowerCase().includes(filterValue.toLowerCase())
                    );
                },

                Cell: ({ cell }) => {
                    const tags: Tag[] = cell.getValue<Tag[]>();
                    const maxDisplayedLength = 20;
                    const maxDisplayedTags = 3;
                    let displayedTags: Tag[] = [];
                    let hiddenTagsCount = 0;
                    let displayedLength = 0;
                    let displayedTagsCount = 0;

                    tags.forEach((tag) => {
                        const tagLength = tag.name.length;

                        if (
                            displayedLength + tagLength <= maxDisplayedLength &&
                            displayedTagsCount < maxDisplayedTags
                        ) {
                            displayedTags.push(tag);
                            displayedLength += tagLength;
                            displayedTagsCount++;
                        } else {
                            hiddenTagsCount++;
                        }
                    });

                    return (
                        <div>
                            {displayedTags.map((tag) => (
                                <Chip
                                    key={tag.id}
                                    label={tag.name}
                                    variant="outlined"
                                    sx={{
                                        mx: "0.2rem",
                                        my: 0,
                                        color: "purple",
                                        borderColor: "purple",
                                        fontWeight: "bold",
                                    }}
                                />
                            ))}
                            {hiddenTagsCount > 0 && (
                                <Chip
                                    label={`+${hiddenTagsCount}`}
                                    variant="outlined"
                                    sx={{
                                        mx: "0.2rem",
                                        my: 0,
                                        color: "purple",
                                        borderColor: "purple",
                                        fontWeight: "bold",
                                    }}
                                />
                            )}
                        </div>
                    );
                },
            },
        ],
        []
    );

    return (
        <MaterialReactTable<WebsiteRecord>
            columns={columns}
            data={data}
            enableColumnFilterModes
            enableGrouping
            enableRowActions
            enableRowSelection
            initialState={{ showColumnFilters: true }}
            positionToolbarAlertBanner="bottom"
            renderDetailPanel={({ row }) => {
                const handleRunNow = async () => {
                    console.log("Run now clicked for id: " + row.original.id);
                    await fetch(`record/rerun/${row.original.id}`, {
                        method: "POST",
                    });
                };
                return (
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
                        </Typography>
                        <div>
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
                        </div>
                        <Typography variant="body1">
                            <strong>Crawl Info:</strong>
                        </Typography>
                        <ul>
                            <li>
                                <Typography variant="body1">
                                    <strong>Entry URL: </strong>
                                    <a
                                        href={row.original.crawlInfo.entryUrl}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                    >
                                        {row.original.crawlInfo.entryUrl}
                                    </a>
                                </Typography>
                            </li>
                            <li>
                                <Typography variant="body1">
                                    <strong>Regex Pattern:</strong>{" "}
                                    {row.original.crawlInfo.regexPattern}
                                </Typography>
                            </li>
                            <li>
                                <Typography variant="body1">
                                    <strong>Periodicity:</strong>{" "}
                                    {row.original.crawlInfo.periodicity}
                                </Typography>
                            </li>
                        </ul>
                        <div style={{ display: "flex", justifyContent: "flex-end" }}>
                            <Button
                                variant="contained"
                                color="primary"
                                onClick={handleRunNow}
                            >
                                Run Now
                            </Button>
                        </div>
                    </Box>
                );
            }}
            renderRowActionMenuItems={({ closeMenu, row }) => [
                <MenuItem
                    key={0}
                    onClick={async () => {
                        const confirmed = window.confirm(
                            "Are you sure you want to delete this record?"
                        );
                        if (confirmed) {
                            let response = await fetch(`record/${row.original.id}`, {
                                method: "DELETE",
                            });

                            if (response.status === 200) {
                                data.splice(row.index, 1);
                                setData([...data]);
                                console.log("Record with id: " + row.original.id + " deleted");
                            } else {
                                console.log("Record with id: " + row.original.id + " could not be deleted. Status code: " + response.status);
                            }
                        }
                        closeMenu();
                    }}
                    sx={{ m: 0 }}
                >
                    <ListItemIcon>
                        <Delete />
                    </ListItemIcon>
                    Delete
                </MenuItem>,
                <MenuItem
                    key={1}
                    onClick={() => {
                        closeMenu();
                        var divided = row.original.crawlInfo.periodicity.split(":");
                        var periodicity = +divided[0] * 60 + +divided[1];
                        const context = {
                            id: row.original.id,
                            name: row.original.label,
                            isActive: row.original.isActive,
                            tags: row.original.tags,
                            periodicity: periodicity, //TODO: fix periodicity //TODD: find out what this TODO is about
                            regexPattern: row.original.crawlInfo.regexPattern,
                            entryUrl: row.original.crawlInfo.entryUrl,
                            isEditing: true,
                        };
                        showEditModalWindow(context);
                    }}
                    sx={{ m: 0 }}
                >
                    <ListItemIcon>
                        <Edit />
                    </ListItemIcon>
                    Edit
                </MenuItem>,
            ]}
            renderTopToolbarCustomActions={({ table }) => {
                const handleDeactivate = () => {
                    table.getSelectedRowModel().flatRows.map(async (row) => {
                        console.log("Deactivating record with id: " + row.original.id);

                        let response = await fetch(`record/stop/${row.original.id}`, {
                            method: "POST",
                        });

                        if (response.status === 200) {
                            console.log("Record with id: " + row.original.id + " deactivated");
                            response = await fetch(`record/${row.original.id}`, {
                                method: "GET",
                            });
                            let newRecord = await response.json();
                            data.forEach((record) => {
                                if (record.id === newRecord.id) {
                                    record.isActive = newRecord.isActive;
                                }
                            });
                            setData([...data]);
                        } else {
                            console.log("Record with id: " + row.original.id + " could not be deactivated");
                        }

                        return null;
                    });
                };

                const handleActivate = () => {
                    table.getSelectedRowModel().flatRows.forEach(async (row) => {
                        console.log("Activating record with id: " + row.original.id);

                        let response = await fetch(`record/run/${row.original.id}`, {
                            method: "POST",
                        });

                        if (response.status === 200) {
                            console.log("Record with id: " + row.original.id + " activated");
                            response = await fetch(`record/${row.original.id}`, {
                                method: "GET",
                            });
                            let newRecord = await response.json();
                            data.forEach((record) => {
                                if (record.id === newRecord.id) {
                                    record.isActive = newRecord.isActive;
                                }
                            });
                            setData([...data]);
                        } else {
                            console.log("Record with id: " + row.original.id + " failed to activate. Status code: " + response.status);
                        }

                        return null;
                    });
                };

                const handleViewGraph = () => {
                    const selectedGraphsIds: number[] = [];

                    table.getSelectedRowModel().flatRows.map((row) => {
                        selectedGraphsIds.push(row.original.id);
                        return null;
                    });

                    navigate("/Graph", { state: { graphsIds: selectedGraphsIds } });
                };

                const handleDelete = () => {
                    const confirmed = window.confirm(
                        "Are you sure you want to delete this record?"
                    );
                    if (confirmed) {
                        table.getSelectedRowModel().flatRows.map(async (row) => {
                            let response = await fetch(`record/${row.original.id}`, {
                                method: "DELETE",
                            });

                            if (response.status === 200) {
                                data.splice(row.index, 1);
                                setData([...data]);
                                console.log("Record with id: " + row.original.id + " deleted");
                            } else {
                                console.log("Record with id: " + row.original.id + " could not be deleted. Status code: " + response.status);
                            }

                            return null;
                        });
                    }
                };

                return (
                    <div style={{ display: "flex", gap: "0.5rem" }}>
                        <Button
                            color="error"
                            disabled={
                                !(table.getIsSomeRowsSelected() || table.getIsAllRowsSelected())
                            }
                            onClick={handleDeactivate}
                            variant="contained"
                        >
                            Deactivate
                        </Button>
                        <Button
                            color="success"
                            disabled={
                                !(table.getIsSomeRowsSelected() || table.getIsAllRowsSelected())
                            }
                            onClick={handleActivate}
                            variant="contained"
                        >
                            Activate
                        </Button>
                        <Button
                            color="primary"
                            disabled={
                                !(table.getIsSomeRowsSelected() || table.getIsAllRowsSelected())
                            }
                            onClick={handleViewGraph}
                            variant="contained"
                        >
                            View Graph
                        </Button>
                        <Button
                            disabled={
                                !(table.getIsSomeRowsSelected() || table.getIsAllRowsSelected())
                            }
                            onClick={handleDelete}
                            variant="contained"
                            color="error"
                            sx={{
                                color: "red",
                                borderColor: "red",
                                backgroundColor: "white",
                            }}
                        >
                            Delete
                            <Delete />
                        </Button>
                    </div>
                );
            }}
        />
    );
};

export default Records;
