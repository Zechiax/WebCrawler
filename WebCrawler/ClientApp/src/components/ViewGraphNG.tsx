import React from "react";
import {DataSet, Network} from "vis-network/standalone/esm/vis-network";
import { useLocation } from "react-router";
import Button from "react-bootstrap/Button";
import "./ViewGraphNG.css";
import { ProgressBar } from 'react-bootstrap';
import { NodeInfo } from "./NodeInfo";

export const ViewGraphNG = (props: any) => {
    const location = useLocation();
    return (
        <ViewGraphNGInternal graphsIds={location.state.graphsIds} {...props} />
    );
};

interface INode {
    id: string;
    label: string;
    color: string;
    started?: string;
    crawlTime?: string;
    fixed?: {
        x: boolean;
        y: boolean;
    };
    value?: number;
    title?: string | HTMLElement;
    mass?: number;
    crawledByRecordIds?: string[];
    shape?: string;
}

interface IEdge {
    id: string;
    from: string;
    to: string;
    arrows: string;
}

interface IGraphData {
    nodes: INode[];
    edges: IEdge[];
}

interface IState {
    graphsIds: number[];
    stabilizationProgress: number;
    errorMessage?: string;
    intervalId?: NodeJS.Timeout;
    liveGraphUrlBase: GraphView;
    staticView: boolean;
    nodeModalContext: {
        show: boolean;
        title: string;
        url: string;
        crawlTime: string;
        crawledByRecords?: string[];
    };
}

enum GraphView {
    Domain = "Record/livegraph/domains/",
    Website = "Record/livegraph/websites/",
}

class ViewGraphNGInternal extends React.Component<
    { graphsIds: number[] },
    IState
> {
    private graphRef = React.createRef<HTMLDivElement>();
    private network?: Network;
    private nodes = new DataSet<INode>();
    private edges = new DataSet<IEdge>();
    private error : boolean = false;
    private recordsDictionary: { [key: string]: any } = {};

    constructor(props: { graphsIds: [] }) {
        super(props);
        this.state = {
            graphsIds: [...this.props.graphsIds],
            stabilizationProgress: 0,
            errorMessage: undefined,
            intervalId: undefined,
            liveGraphUrlBase: GraphView.Domain,
            staticView: false,
            nodeModalContext: {
                show: false,
                title: "",
                url: "",
                crawlTime: "",
                crawledByRecords: [],
            },
        };
    }

    async componentDidMount() {
        this.edges.on("add", this.edgeAddedHandler.bind(this));

        console.log("Component mounted, loading graphs with ids: " + this.state.graphsIds);
        await this.updateGraphAsync().then(() => {
            console.log("Graph data loaded");
            if (!this.error) {
                this.initializeGraph();
                console.log("Graph initialized");
            }
            else {
                return;
            }
        });

        console.log("Starting periodic graph update");
        // Start the interval timer to update the graph every 5 seconds
        this.startLiveUpdate();
    }

    edgeAddedHandler(event: any, properties: { items: string[] }) {
        console.log("Edges added ( " + properties.items.length + " )");
        for (const edgeId of properties.items) {
            const edge = this.edges.get(edgeId);
            const fromNode = this.nodes.get(edge.from);
            const toNode = this.nodes.get(edge.to);

            if (fromNode) {
                fromNode.value = (fromNode.value || 1) + 1;
                fromNode.mass = (fromNode.mass || 1) + 1;
                this.nodes.update(fromNode);
            }

            if (toNode) {
                toNode.value = (toNode.value || 1) + 1;
                toNode.mass = (toNode.mass || 1) + 1;
                this.nodes.update(toNode);
            }
        }
    }

    async componentDidUpdate(prevProps: {}, prevState: IState) {
        if (prevState.liveGraphUrlBase !== this.state.liveGraphUrlBase) {
            console.log("Graph view changed to: " + this.state.liveGraphUrlBase);
            this.stopLiveUpdate();
            this.clearGraph();
            this.removeGraph();
            await this.updateGraphAsync();
            this.initializeGraph();
            this.setState({ staticView: false });
            this.startLiveUpdate();
        }
    }        

    clearGraph() {
        this.edges.clear();
        this.nodes.clear();
    }

    async updateGraphAsync() {
        console.log(this.state.graphsIds);
        const graphsJson = [];

        for (const id of this.state.graphsIds) {
            const response = await fetch(this.state.liveGraphUrlBase + id);

            if (response.ok) {
                const graphJson = await response.json();
                graphJson["websiteRecordId"] = id;
                graphsJson.push(graphJson);
            } else {
                console.log("Error fetching graph: " + id);
                this.showErrorMessage();
                return;
            }
        }

        const graphData = await this.convertGraphsJsonToVisJsonAsync(graphsJson);

        this.nodes.update(graphData.nodes);
        this.edges.update(graphData.edges);
    }

    showErrorMessage() {
        this.error = true;
        console.log("Showing error message");
        this.setState({
            errorMessage: "The graph for this record could not be fetched. Please try again later.",
        });
    }

    async convertGraphsJsonToVisJsonAsync(
        graphsJson: any[]
    ): Promise<IGraphData> {
        const graphData: IGraphData = {
            nodes: [],
            edges: [],
        };

        for (const graphJson of graphsJson) {
            console.log("Fetching record for graph: " + graphJson.websiteRecordId);
            const recordForGraphResponse = await (
                await fetch(`/Record/${graphJson.websiteRecordId}`)
            );

            if (!recordForGraphResponse.ok) {
                console.log(
                    "Error fetching record for graph: " + graphJson.websiteRecordId
                );
                this.showErrorMessage();
                return;
            }

            const recordId = graphJson.websiteRecordId;

            const recordForGraph = await recordForGraphResponse.json();

            // We update the dictionary of records with the new record
            this.recordsDictionary[recordId] = recordForGraph;

            let firstNode : boolean = true;

            for (const node of graphJson.Graph) {
                const color = new RegExp(
                    recordForGraph.crawlInfo.regexPattern
                ).test(node.Url)
                    ? this.stringToColour(recordForGraph.label)
                    : "orange";

                const alreadyPresentNode = graphData.nodes.find(
                    (n) => n.id === node.Url
                );

                let element = "Url: " + node.Url + "\n";

                if (alreadyPresentNode) {
                    if (
                        !alreadyPresentNode.crawledByRecordIds.includes(
                            recordId
                        )
                    ) {
                        alreadyPresentNode.crawledByRecordIds.push(
                            recordId
                        );
                    }
                    if (
                        Date.parse(recordForGraph.crawlInfo.lastExecution.started) >
                        Date.parse(alreadyPresentNode.started)
                    ) {
                        alreadyPresentNode.label = node.Title;
                        alreadyPresentNode.color = color;
                        alreadyPresentNode.title = element;
                        alreadyPresentNode.started = recordForGraph.crawlInfo.lastExecution.started;
                        alreadyPresentNode.crawlTime = node.CrawlTime;
                    }
                } else {
                    const newNode : INode = {
                        id: node.Url,
                        label: node.Title,
                        color: color,
                        started: recordForGraph.crawlInfo.lastExecution.started,
                        crawlTime: node.CrawlTime,
                        title: element,
                        crawledByRecordIds: [recordId],
                    }

                    if (firstNode) {
                        newNode.shape = "square"
                        firstNode = false;
                    }

                    graphData.nodes.push(newNode);
                }

                for (const neighbourUrl of node.Neighbours) {
                    // Ids are from url to url
                    const id = `${node.Url} -> ${neighbourUrl}`;
                    const id_reverse = `${neighbourUrl} -> ${node.Url}`;

                    // We add arrows from if the node in reverse is present
                    const alreadyPresentEdge = graphData.edges.find(
                        (e) => e.id === id_reverse
                    );

                    if (alreadyPresentEdge) {
                        alreadyPresentEdge.arrows += ",from";
                        continue;
                    }

                    graphData.edges.push({
                        id: `${id}`,
                        from: node.Url,
                        to: neighbourUrl,
                        arrows: "to",
                    });
                }
            }
        }

        return graphData;
    }

    removeGraph() {
        console.log("Removing graph");
        if (this.network) {
            this.network.destroy();
        }
    }

    startLiveUpdate(interval : number = 5000) {
        if (this.state.intervalId) {
            this.stopLiveUpdate();
        }

        console.log("Starting live update");
        const intervalId = setInterval(async () => {
            await this.updateGraphAsync();
        }, interval);

        // Store the intervalId in the state
        this.setState({intervalId});
    }

    stopLiveUpdate() {
        console.log("Stopping live update");
        // Clear the interval right before component unmount
        if (this.state.intervalId) {
            clearInterval(this.state.intervalId);
            this.setState({intervalId: undefined});
        }
    }

    stringToColour = (str: string) => {
        let hash = 0;
        for (let i = 0; i < str.length; i++) {
            // tslint:disable-next-line:no-bitwise
            hash = str.charCodeAt(i) + ((hash << 5) - hash);
        }
        let colour = "#";
        for (let i = 0; i < 3; i++) {
            // tslint:disable-next-line:no-bitwise
            const value = (hash >> (i * 8)) & 0xff;
            colour += ("00" + value.toString(16)).substr(-2);
        }
        return colour;
    };

    componentWillUnmount() {
        // Clear the interval right before component unmount
        this.stopLiveUpdate();
        this.clearGraph();
        this.removeGraph();
    }

    initializeGraph() {
        // provide the data in the vis format
        const data = {
            nodes: this.nodes,
            edges: this.edges,
        };

        const options = {
            physics: {
                stabilization: {
                    enabled: true,
                    iterations: 1000,
                },
                barnesHut: {
                    gravitationalConstant: -80000,
                    springConstant: 0.001,
                    springLength: 200,
                },
                timestep: 2.5,
                adaptiveTimestep: true,
            },
            layout: {
                improvedLayout: false,
                hierarchical: false,
            },
            edges: {
                smooth: false,
                color: {
                    color: "#E6E5E6",
                    inherit: false,
                },
            },
            nodes: {
                shape: "dot",
                scaling: {
                    min: 100,
                    max: 500,
                },
                font: {
                    size: 12,
                    face: "Tahoma",
                },
            },
            interaction: {
                hover: true,
                tooltipDelay: 200,
                hideEdgesOnDrag: true,
                hideEdgesOnZoom: true,
                navigationButtons: true,
                keyboard: true,
            },
            height: "100%",
        };

        // initialize your network!
        this.network = new Network(this.graphRef.current, data, options);

        this.network.on("stabilizationProgress", (params) => {
            this.setState({
                stabilizationProgress: (params.iterations / params.total) * 100,
            });
            console.log("Stabilization progress: " + this.state.stabilizationProgress);
        });

        this.network.on("stabilizationIterationsDone", () => {
            this.setState({stabilizationProgress: 100});
            console.log("Stabilization done");
            this.network!.fit();
        });

        this.network.on("click", this.clickNodeEventHandler);
    }

    addHttpOrHttps(url: string): string {
        if (!url.startsWith("http://") && !url.startsWith("https://")) {
            return `https://${url}`;
        }
        return url;
    }

    clickNodeEventHandler = (params: any) => {
        if (params.nodes.length === 0) {
            return;
        }
        const id: string = params.nodes[0];
        let node = this.nodes.get(id);

        let crawledByRecords = node.crawledByRecordIds.map((recordId) => {
            return this.recordsDictionary[recordId];
        });

        console.log(crawledByRecords);

        const url = this.addHttpOrHttps(node.id);

        this.setState({
            nodeModalContext: {
                show: true,
                title: node.label,
                url: url,
                crawlTime: node.crawlTime,
                crawledByRecords: [...crawledByRecords],
            }
        })
    }

    render() {

        if (this.state.errorMessage) {
            return (
                // Possible style style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}
                <div className="alert alert-danger">
                    <strong>{this.state.errorMessage}</strong>
                </div>
            );
        }

        return (
            <>
                {this.state.stabilizationProgress < 100 && (
                    <div
                        style={{
                            position: "absolute",
                            top: "50%",
                            left: "25%",
                            width: "50%",
                        }}
                    >
                        <span
                            style={{
                                fontWeight: "bold",
                                fontSize: "20px",
                            }}
                        >
                            Generating { this.state.liveGraphUrlBase === GraphView.Domain ? "Domain view" : "Website view" } graph...
                        </span>
                        <ProgressBar
                            striped
                            variant="info"
                            now={this.state.stabilizationProgress}
                            label={`${this.state.stabilizationProgress}%`}
                        />
                    </div>
                )}

                <div ref={this.graphRef} className="full-height" />
                
                <div
                    style={{
                        display: "grid",
                        gridTemplateAreas: "'top''middle''bottom'",
                        position: "absolute",
                        top: "70px",
                        right: "10px",
                        gap: "10px",
                    }}
                >
                    <Button
                        style={{
                            gridArea: "top",
                        }}
                        onClick={() => {
                            if (this.state.liveGraphUrlBase === GraphView.Domain) {
                                console.log("Changing to website view");
                                this.setState({ liveGraphUrlBase: GraphView.Website });
                            } else {
                                console.log("Changing to domain view");
                                this.setState({ liveGraphUrlBase: GraphView.Domain });
                            }
                        }}

                    >
                        { this.state.liveGraphUrlBase === GraphView.Domain ? "Website view" : "Domain view" }
                    </Button>
                    <Button
                        style={{
                            gridArea: "middle",
                        }}
                        onClick={() => {
                            if (this.state.staticView) {
                                this.setState({ staticView: false });
                                this.startLiveUpdate();
                            } else {
                                this.setState({ staticView: true });
                                this.stopLiveUpdate();
                            }
                        }}

                    >
                        {this.state.staticView ? "Dynamic View" : "Static View"}
                    </Button>
                    <Button
                        style={{
                            gridArea: "bottom",
                            visibility: this.state.staticView ? "visible" : "hidden",
                        }}
                        onClick={() => {
                            this.updateGraphAsync();
                        }}
                    >
                        Update Graph
                    </Button>

                </div>

                <NodeInfo
                    show={this.state.nodeModalContext.show}
                    title={this.state.nodeModalContext.title}
                    url={this.state.nodeModalContext.url}
                    crawlTime={this.state.nodeModalContext.crawlTime}
                    records={this.state.nodeModalContext.crawledByRecords}
                    onHide={() => {
                        this.setState({
                            nodeModalContext: {
                                show: false,
                                title: "",
                                url: "",
                                crawlTime: "",
                                crawledByRecords: []
                            }
                        })
                    }}
                />
            </>
        );
    }
}

export default ViewGraphNG;
