import React, { Component } from "react";
import { useLocation } from "react-router";
import Button from "react-bootstrap/Button";
import * as d3 from "d3";
import Modal from "react-bootstrap/Modal";
import ListGroup from "react-bootstrap/ListGroup";
import { Label } from "reactstrap";
import { CreateWebsiteRecordModalWindow } from "./CreateWebsiteRecordModalWindow";

export const ViewGraph = (props) => {
  const location = useLocation();
  return (
    <ViewGraphInternal
      ids={location.state.ids}
      height="800px"
      width="100%"
      {...props}
    />
  );
};

class ViewGraphInternal extends Component {
  constructor(props) {
    super(props);
    this.state = {
      graphsIds: [...props.ids],
      nodes: [],
      links: [],
      isWebsitesView: false,
      isLiveView: false,
      currentlySelectedNodeInfo: [],
      nodeInfo: {
        show: false,
        title: null,
        url: null,
        crawlTime: null,
        records: [],
      },
      createNewWebsiteRecord: {
        show: false,
      },
    };
  }

  renderGraph(graph) {
    const nodes = graph.nodes.map((node) => Object.create(node));
    const links = graph.links.map((link) => Object.create(link));

    const self = this;

    const nodeInfo = d3.select(".content .nodeInfo");

    const svg = d3
      .select("svg")
      .attr("width", this.props.width)
      .attr("height", this.props.height)
      .call(
        d3.zoom().on("zoom", function (e) {
          d3.selectAll("svg g").attr("transform", e.transform);
        })
      );

    const simulation = d3
      .forceSimulation()
      .force("charge", d3.forceManyBody().strength(-5))
      .force(
        "center",
        d3.forceCenter(
          350, //TODO - calculate center somehow from svg dimension
          350
        )
      );

    // TODO: Attempt to implement dragging, but doesn't work for some reason
    svg.call(
      d3
        .drag()
        .container(svg)
        .subject((e) => simulation.find(e.x, e.y))
        .on("start", (e) => {
          if (!e.active) {
            simulation.alphaTarget(0.3).restart();
          }

          e.subject.fx = e.subject.x;
          e.subject.fy = e.subject.y;
        })
        .on("drag", (e) => {
          e.subject.fx = e.x;
          e.subject.fy = e.y;
        })
        .on("end", (e) => {
          if (!e.active) {
            simulation.alphaTarget(0);
          }

          e.subject.fx = null;
          e.subject.fy = null;
        })
    );
    //

    const linkElements = svg
      .append("g")
      .selectAll("line")
      .data(links)
      .enter()
      .append("line")
      .attr("stroke-width", 1)
      .attr("stroke", "#E5E5E5");

    const nodeElements = svg
      .append("g")
      .selectAll("circle")
      .data(nodes)
      .enter()
      .append("circle")
      .attr("id", function (d) {
        return d.url;
      })
      .attr("class", ".node")
      .attr("r", 3)
      .attr("fill", function (d) {
        return d.color;
      })
      .on("click", function (e, d, i) {
        self.setState({
          nodeInfo: {
            show: true,
            title: d.title,
            url: d.url,
            crawlTime: d.crawlTime,
            records: d.inWhichRecords,
          },
        });
      })
      .on("mouseover", function (e, d, i) {
        nodeInfo
          .append("div")
          .style("font-size", "12px")
          .append("ul")
          .append("li")
          .text("Title: " + (d.title ? d.title : "???"))
          .append("li")
          .text("Url: " + d.url)
          .append("li")
          .text("Crawl time: " + d.crawlTime)
          .append("li")
          .text(
            d.inWhichRecords.map((record) => `${record.label}(${record.id})`)
          );

        d3.select(this).attr("fill", "red");
      })
      .on("mouseout", function (e, d, i) {
        nodeInfo.selectAll("ul").remove();
        svg
          .selectAll("circle")
          .filter(function () {
            return d3.select(this).attr("fill") === "red";
          })
          .attr("fill", d.color);
      });

    simulation.nodes(nodes).on("tick", () => {
      nodeElements.attr("cx", (node) => node.x).attr("cy", (node) => node.y);

      linkElements
        .attr("x1", (link) => link.source.x)
        .attr("y1", (link) => link.source.y)
        .attr("x2", (link) => link.target.x)
        .attr("y2", (link) => link.target.y);
    });

    simulation.force(
      "link",
      d3
        .forceLink(links)
        .id((link) => link.url)
        .distance(20)
    );
  }

  componentDidMount() {
    this.fetchAndRerenderGraph("Record/livegraph/domains/");
  }

  fetchAndRerenderGraph(urlbase) {
    console.log(urlbase);
    this.getGraphAsync(urlbase).then((graph) => this.renderGraph(graph));
  }

  async getGraphAsync(urlbase) {
    console.log(urlbase);

    const graphsJson = [];

    console.log(this.state.graphsIds);

    for (const id of this.state.graphsIds) {
      const response = await fetch(urlbase + id);

      if (response.ok) {
        const graphJson = await response.json();
        graphJson["websiteRecordId"] = id;
        console.log(graphJson);
        graphsJson.push(graphJson);
      }
    }

    const graph = await ViewGraphInternal.convertGraphsJsonToD3JsonAsync(
      graphsJson
    );
    return graph;
  }

  static async convertGraphsJsonToD3JsonAsync(graphsJson) {
    const graph = {
      nodes: [],
      links: [],
    };

    for (const graphJson of graphsJson) {
      const recordForGraph = await (
        await fetch(`/Record/${graphJson.websiteRecordId}`)
      ).json();
      console.log("record for graph");
      console.log(recordForGraph);

      for (const node of graphJson.Graph) {
        // if regex doesn't match is orange
        const color = new RegExp(recordForGraph.crawlInfo.regexPattern).test(
          node.Url
        )
          ? ViewGraphInternal.stringToColour(recordForGraph.label)
          : "orange";

        const alreadyPresentNode = graph.nodes.find((n) => n.url === node.Url);

        // present but this one is newer
        if (alreadyPresentNode) {
          if (
            Date.parse(recordForGraph.crawlInfo.lastExecution.started) >
            alreadyPresentNode.started
          ) {
            console.log(alreadyPresentNode);
            // overwrite with latest data
            alreadyPresentNode.url = node.Url;
            alreadyPresentNode.title = node.Title;
            alreadyPresentNode.crawlTime = node.CrawlTime;
            alreadyPresentNode.started = Date.parse(
              recordForGraph.crawlInfo.lastExecution.started
            );

            alreadyPresentNode.color = color;
          }

          alreadyPresentNode.inWhichRecords.push(recordForGraph.label);
        } else {
          graph.nodes.push({
            url: node.Url,
            title: node.Title,
            crawlTime: node.CrawlTime,
            started: Date.parse(recordForGraph.crawlInfo.lastExecution.started),
            inWhichRecords: [
              { label: recordForGraph.label, id: recordForGraph.id },
            ],
            color: color,
          });
        }

        for (const neighbourUrl of node.Neighbours) {
          graph.links.push({
            source: node.Url,
            target: neighbourUrl,
          });
        }
      }
    }

    console.log("converted graph for d3");
    console.log(graph);
    return graph;
  }

  //https://stackoverflow.com/questions/3426404/create-a-hexadecimal-colour-based-on-a-string-with-javascript
  static stringToColour(str) {
    var hash = 0;
    for (let i = 0; i < str.length; i++) {
      hash = str.charCodeAt(i) + ((hash << 5) - hash);
    }
    var colour = "#";
    for (let i = 0; i < 3; i++) {
      var value = (hash >> (i * 8)) & 0xff;
      colour += ("00" + value.toString(16)).substr(-2);
    }
    return colour;
  }

  updateGraph(urlbase) {
    // TODO: attempt for live update instead of rererending the whole graph all the time - not working
    /*     this.setState((state, props) => ({
      nodes: [
        ...nodes,
        {
          url: "borek",
        },
      ],
    }));

    let nodes = d3.selectAll("circle").data(this.state.nodes);
    const newNodesEnter = nodes
      .enter()
      .append("circle")
      .attr("id", function (d) {
        return d.url;
      })
      .attr("class", ".node")
      .attr("r", 3)
      .attr("fill", function (d) {
        return d.color;
      });

    nodes = newNodesEnter.merge(nodes);
    // */

    this.deleteGraph();
    this.fetchAndRerenderGraph(urlbase);
  }

  deleteGraph() {
    d3.selectAll("circle").remove();
    d3.selectAll("line").remove();
  }

  render() {
    return (
      <>
        <div className="content">
          <div
            className="nodeInfo"
            style={{
              position: "absolute",
              zIndex: 1,
            }}
          />
          <div
            style={{
              position: "absolute",
              left: "85vw",
            }}
          >
            <Button
              style={{
                width: "130px",
              }}
              variant="primary"
              onClick={() => {
                this.setState((oldState, props) => {
                  let state = {
                    ...oldState,
                  };

                  state.isWebsitesView = !oldState.isWebsitesView;
                  return state;
                });

                this.deleteGraph();
                const urlbase = this.state.isWebsitesView
                  ? "/Record/livegraph/domains/"
                  : "/Record/livegraph/websites/";
                this.fetchAndRerenderGraph(urlbase);
              }}
            >
              {this.state.isWebsitesView ? "Websites view" : "Domains view"}
            </Button>
            <Button
              style={{
                marginTop: "10px",
                width: "130px",
              }}
              variant="primary"
              onClick={() => {
                this.setState((oldState, props) => {
                  let state = {
                    ...oldState,
                  };

                  state.isLiveView = !oldState.isLiveView;
                  return state;
                });
              }}
            >
              {this.state.isLiveView ? "Live view" : "Static view"}
            </Button>
          </div>
          {!this.state.isLiveView ? (
            <div
              style={{
                position: "fixed",
                left: "85vw",
                bottom: "2vw",
              }}
            >
              <Button
                onClick={() => {
                  const urlbase = !this.state.isWebsitesView
                    ? "/Record/livegraph/domains/"
                    : "/Record/livegraph/websites/";
                  this.updateGraph(urlbase);
                }}
              >
                Update Graph
              </Button>
            </div>
          ) : (
            <div></div>
          )}
          <svg
            style={{
              zIndex: 0,
            }}
          ></svg>
        </div>

        <Modal
          show={this.state.nodeInfo.show}
          onHide={() => {
            this.setState((oldState, props) => {
              let state = {
                ...oldState,
              };

              state.nodeInfo = {
                show: false,
              };

              return state;
            });
          }}
          size="lg"
          centered
        >
          <Modal.Header closeButton>Node Info</Modal.Header>
          <ListGroup
            style={{
              margin: 10,
            }}
          >
            <ListGroup.Item>title: {this.state.nodeInfo.title}</ListGroup.Item>
            <ListGroup.Item>url: {this.state.nodeInfo.url}</ListGroup.Item>
            <ListGroup.Item>
              crawl time: {this.state.nodeInfo.crawlTime}
            </ListGroup.Item>
            <Label
              style={{
                marginTop: "20px",
                marginLeft: "16px",
              }}
            >
              Website records
            </Label>
            <ListGroup>
              {this.state.nodeInfo.records?.map((record, i) => {
                return (
                  // TODO: add arrow that will reveal more info about website record - component from home.js table
                  // HOW: we have id, so just fetch the record from server and bind it to the props
                  <ListGroup.Item
                    key={i}
                    variant="light"
                  >{`${record.label}(${record.id})`}</ListGroup.Item>
                );
              })}
            </ListGroup>
          </ListGroup>
          <div
            style={{
              margin: 10,
            }}
          >
            <Button
              onClick={() => {
                this.setState((oldState, props) => {
                  let state = {
                    ...oldState,
                  };

                  state.createNewWebsiteRecord = {
                    show: true,
                  };

                  return state;
                });
              }}
              variant="primary"
              style={{
                fontSize: 12,
                width: 300,
              }}
            >
              Create new website record from this node
            </Button>
          </div>
        </Modal>

        <CreateWebsiteRecordModalWindow
          show={this.state.createNewWebsiteRecord.show}
          passCreatedRecordId={(id) => {
            this.setState((oldState, props) => ({
              graphsIds: [...oldState.graphsIds, id],
            }));
          }}
          urlPresetValue={this.state.nodeInfo.url}
          labelPresetValue={this.state.nodeInfo.title}
          onClose={() =>
            this.setState((oldState, props) => {
              let state = {
                ...oldState,
              };

              state.createNewWebsiteRecord = {
                show: false,
              };

              return state;
            })
          }
        />
      </>
    );
  }
}
